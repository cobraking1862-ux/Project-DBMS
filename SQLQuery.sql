-- ======================================================================
-- PROJECT: University Event & Competition Management System
-- TARGET: MS SQL Server (SSMS)
-- DESCRIPTION: Core Schema, Constraints, Views, Procedures, and Triggers
-- ======================================================================

-- 1. DATABASE CREATION
-- ======================================================================
CREATE DATABASE EventManagementDB;
GO

USE EventManagementDB;
GO

-- ======================================================================
-- 2. MASTER DATA TABLES
-- ======================================================================

CREATE TABLE Students (
    RollNumber VARCHAR(20) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Department VARCHAR(50) NOT NULL,
    Semester INT NOT NULL,
    IsActive BIT DEFAULT 1
);
GO

CREATE TABLE Teachers (
    TeacherID INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Department VARCHAR(50) NOT NULL,
    Designation VARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1
);
GO

-- ======================================================================
-- 3. TRANSACTIONAL DATA TABLES
-- ======================================================================

CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName VARCHAR(100) NOT NULL,
    Status VARCHAR(30) DEFAULT 'Draft', -- Draft, Live, Submissions Closed, Evaluation, Closed
    TargetDepartment VARCHAR(50) NULL,  -- [Flat Rule] NULL means open to all departments
    MinSemester INT NULL,               -- [Flat Rule] 
    MaxSemester INT NULL,               -- [Flat Rule] 
    IsActive BIT DEFAULT 1
);
GO

CREATE TABLE EventJudges (
    JudgeAssignmentID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL FOREIGN KEY REFERENCES Events(EventID),
    TeacherID INT NOT NULL FOREIGN KEY REFERENCES Teachers(TeacherID),
    IsActive BIT DEFAULT 1,
    CONSTRAINT UQ_Event_Teacher UNIQUE (EventID, TeacherID)
);
GO

CREATE TABLE EventRegistrations (
    RegID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL FOREIGN KEY REFERENCES Events(EventID),
    RollNumber VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES Students(RollNumber),
    ApprovalStatus VARCHAR(20) DEFAULT 'Pending',
    IsActive BIT DEFAULT 1,
    CONSTRAINT UQ_Event_RollNumber UNIQUE (EventID, RollNumber)
);
GO

CREATE TABLE Submissions (
    SubmissionID INT IDENTITY(1,1) PRIMARY KEY,
    RegID INT NOT NULL FOREIGN KEY REFERENCES EventRegistrations(RegID),
    MaterialLink VARCHAR(255) NOT NULL,
    SubmissionDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

CREATE TABLE Evaluations (
    EvalID INT IDENTITY(1,1) PRIMARY KEY,
    SubmissionID INT NOT NULL FOREIGN KEY REFERENCES Submissions(SubmissionID),
    JudgeAssignmentID INT NOT NULL FOREIGN KEY REFERENCES EventJudges(JudgeAssignmentID),
    Score DECIMAL(5,2) NOT NULL,
    Remarks VARCHAR(500) NULL,
    IsActive BIT DEFAULT 1
);
GO

CREATE TABLE ScoreAudits (
    AuditID INT IDENTITY(1,1) PRIMARY KEY,
    EvalID INT NOT NULL FOREIGN KEY REFERENCES Evaluations(EvalID),
    OldScore DECIMAL(5,2) NOT NULL,
    NewScore DECIMAL(5,2) NOT NULL,
    ChangedDate DATETIME DEFAULT GETDATE()
);
GO

-- ======================================================================
-- 4. VIEWS
-- ======================================================================

-- View: vw_AdminDashboard_Summary
-- Purpose: Aggregates real-time statistics for the WPF Admin Dashboard.
CREATE VIEW vw_AdminDashboard_Summary AS
SELECT 
    e.EventID,
    e.EventName,
    e.Status,
    COUNT(DISTINCT er.RegID) AS TotalApplicants,
    SUM(CASE WHEN er.ApprovalStatus = 'Approved' THEN 1 ELSE 0 END) AS ApprovedParticipants,
    AVG(ev.Score) AS AverageScore
FROM Events e
LEFT JOIN EventRegistrations er ON e.EventID = er.EventID AND er.IsActive = 1
LEFT JOIN Submissions s ON er.RegID = s.RegID AND s.IsActive = 1
LEFT JOIN Evaluations ev ON s.SubmissionID = ev.SubmissionID AND ev.IsActive = 1
WHERE e.IsActive = 1
GROUP BY e.EventID, e.EventName, e.Status;
GO

-- ======================================================================
-- 5. STORED PROCEDURES
-- ======================================================================

-- Procedure: sp_RegisterForEvent
-- Purpose: Automates eligibility checks based on flat rules.
CREATE PROCEDURE sp_RegisterForEvent
    @EventID INT,
    @RollNumber VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StudentDept VARCHAR(50), @StudentSem INT;
    DECLARE @TargetDept VARCHAR(50), @MinSem INT, @MaxSem INT, @EventStatus VARCHAR(30);
    DECLARE @IsEligible BIT = 1;

    -- Retrieve student and event rule data
    SELECT @StudentDept = Department, @StudentSem = Semester FROM Students WHERE RollNumber = @RollNumber AND IsActive = 1;
    SELECT @TargetDept = TargetDepartment, @MinSem = MinSemester, @MaxSem = MaxSemester, @EventStatus = Status 
    FROM Events WHERE EventID = @EventID AND IsActive = 1;

    -- 1. Ensure event is actually open
    IF @EventStatus <> 'Live'
    BEGIN
        RAISERROR('Event is not currently open for registration.', 16, 1);
        RETURN;
    END

    -- 2. Process Flat Rules
    IF @TargetDept IS NOT NULL AND @StudentDept <> @TargetDept SET @IsEligible = 0;
    IF @MinSem IS NOT NULL AND @StudentSem < @MinSem SET @IsEligible = 0;
    IF @MaxSem IS NOT NULL AND @StudentSem > @MaxSem SET @IsEligible = 0;

    -- 3. Execute logic
    IF @IsEligible = 1
    BEGIN
        INSERT INTO EventRegistrations (EventID, RollNumber, ApprovalStatus)
        VALUES (@EventID, @RollNumber, 'Approved');
    END
    ELSE
    BEGIN
        RAISERROR('Eligibility Denied: Student does not meet the department or semester requirements for this event.', 16, 1);
    END
END;
GO

-- ======================================================================
-- 6. TRIGGERS
-- ======================================================================

-- Trigger 1: trg_Audit_EvaluationScores
-- Purpose: Ensures scoring transparency by logging changed grades.
CREATE TRIGGER trg_Audit_EvaluationScores
ON Evaluations
AFTER UPDATE
AS
BEGIN
    IF UPDATE(Score)
    BEGIN
        INSERT INTO ScoreAudits (EvalID, OldScore, NewScore)
        SELECT i.EvalID, d.Score, i.Score
        FROM inserted i
        INNER JOIN deleted d ON i.EvalID = d.EvalID
        WHERE i.Score <> d.Score; -- Only audit if the score actually changed
    END
END;
GO

-- Trigger 2: trg_ArchiveGuard_Registrations
-- Purpose: Blocks changes to Registrations if the Event is Closed.
CREATE TRIGGER trg_ArchiveGuard_Registrations
ON EventRegistrations
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM Events e
        INNER JOIN inserted i ON e.EventID = i.EventID WHERE e.Status = 'Closed'
    ) OR EXISTS (
        SELECT 1 FROM Events e
        INNER JOIN deleted d ON e.EventID = d.EventID WHERE e.Status = 'Closed'
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Action Failed: The target event is archived and strictly read-only.', 16, 1);
    END
END;
GO

-- Trigger 3: trg_ArchiveGuard_Submissions
-- Purpose: Blocks changes to Submissions if the Event is Closed.
CREATE TRIGGER trg_ArchiveGuard_Submissions
ON Submissions
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM Events e
        INNER JOIN EventRegistrations er ON e.EventID = er.EventID
        INNER JOIN inserted i ON er.RegID = i.RegID WHERE e.Status = 'Closed'
    ) OR EXISTS (
        SELECT 1 FROM Events e
        INNER JOIN EventRegistrations er ON e.EventID = er.EventID
        INNER JOIN deleted d ON er.RegID = d.RegID WHERE e.Status = 'Closed'
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Action Failed: The target event is archived and strictly read-only.', 16, 1);
    END
END;
GO

-- Trigger 4: trg_ArchiveGuard_Evaluations
-- Purpose: Blocks changes to Evaluations if the Event is Closed.
CREATE TRIGGER trg_ArchiveGuard_Evaluations
ON Evaluations
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM Events e
        INNER JOIN EventRegistrations er ON e.EventID = er.EventID
        INNER JOIN Submissions s ON er.RegID = s.RegID
        INNER JOIN inserted i ON s.SubmissionID = i.SubmissionID WHERE e.Status = 'Closed'
    ) OR EXISTS (
        SELECT 1 FROM Events e
        INNER JOIN EventRegistrations er ON e.EventID = er.EventID
        INNER JOIN Submissions s ON er.RegID = s.RegID
        INNER JOIN deleted d ON s.SubmissionID = d.SubmissionID WHERE e.Status = 'Closed'
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Action Failed: The target event is archived and strictly read-only.', 16, 1);
    END
END;
GO

PRINT 'Database EventManagementDB and all core schemas created successfully.';