-- ======================================================================
-- PROJECT: University Event & Competition Management System
-- TARGET: MS SQL Server (SSMS)
-- DESCRIPTION: Sample Data Population (Corrected Lifecycle)
-- ======================================================================

USE EventManagementDB;
GO

-- ======================================================================
-- 1. SYSTEM ADMIN TABLE 
-- ======================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemAdmins')
BEGIN
    CREATE TABLE SystemAdmins (
        AdminID INT IDENTITY(1,1) PRIMARY KEY,
        Username VARCHAR(50) UNIQUE NOT NULL,
        Password VARCHAR(255) NOT NULL
    );
END
GO

INSERT INTO SystemAdmins (Username, Password) 
VALUES ('root', 'sudo');
GO

-- ======================================================================
-- 2. POPULATE TEACHERS
-- ======================================================================
INSERT INTO Teachers (FullName, Department, Designation) VALUES
('Saleem Vighio', 'Computer Science', 'Professor'),
('Bushra Khan', 'Computer Science', 'Assistant Professor'),
('Aijaz Ahmed', 'Mathematics', 'Lecturer'),
('Shamshad Lakho', 'Computer Science', 'Associate Professor'),
('Nadia Hassan', 'Software Engineering', 'Assistant Professor'),
('Tariq Mahmood', 'Computer Science', 'Lecturer'),
('Farhan Ali', 'Information Technology', 'Professor'),
('Sadia Qureshi', 'Software Engineering', 'Lecturer'),
('Imran Soomro', 'Mathematics', 'Associate Professor'),
('Hina Abbasi', 'Computer Science', 'Assistant Professor');
GO

-- ======================================================================
-- 3. POPULATE STUDENTS (Varied for Rule Testing)
-- ======================================================================
INSERT INTO Students (RollNumber, FullName, Department, Semester) VALUES
('24BSCS01', 'Muhammad Masoom', 'Computer Science', 4),
('24BSCS02', 'Ali Raza', 'Software Engineering', 2),
('24BSCS03', 'Fatima Bilal', 'Computer Science', 6), 
('24BSCS04', 'Zainab Tariq', 'Information Technology', 1),
('24BSCS05', 'Kamran Ali', 'Computer Science', 8), 
('24BSCS06', 'Sana Baloch', 'Mathematics', 3),
('24BSCS07', 'Bilal Memon', 'Data Science', 5),
('24BSCS08', 'Ayesha Siddiqui', 'Software Engineering', 7),
('24BSCS09', 'Usman Ghani', 'Information Technology', 2),
('24BSCS10', 'Hira Soomro', 'Cyber Security', 4),
('24BSCS11', 'Saad Hussain', 'Mathematics', 8),
('24BSCS12', 'Kiran Shah', 'Artificial Intelligence', 1),
('24BSCS13', 'Fahad Mustafa', 'Data Science', 3),
('24BSCS14', 'Nida Chaudhry', 'Software Engineering', 5),
('24BSCS15', 'Omer Farooq', 'Information Technology', 7),
('24BSCS16', 'Rabia Hassan', 'Computer Science', 2),
('24BSCS17', 'Zohaib Ahmed', 'Mathematics', 4),
('24BSCS18', 'Madiha Rajput', 'Cyber Security', 6),
('24BSCS19', 'Taha Qureshi', 'Artificial Intelligence', 8),
('24BSCS20', 'Iqra Khan', 'Software Engineering', 1),
('24BSCS21', 'Shahzaib Ali', 'Information Technology', 3),
('24BSCS22', 'Mariam Shafiq', 'Computer Science', 5),
('24BSCS23', 'Waqas Jamil', 'Mathematics', 7),
('24BSCS24', 'Anum Malik', 'Cyber Security', 2),
('24BSCS25', 'Danish Nawaz', 'Data Science', 4),
('24BSCS26', 'Zara Sheikh', 'Software Engineering', 6),
('24BSCS27', 'Faisal Rehman', 'Information Technology', 8),
('24BSCS28', 'Saba Qamar', 'Computer Science', 1),
('24BSCS29', 'Hassan Ali', 'Artificial Intelligence', 3),
('24BSCS30', 'Khadija Tul Kubra', 'Cyber Security', 5);
GO

-- ======================================================================
-- 4. POPULATE EVENTS 
-- ======================================================================
-- NOTE: We insert the Science Fair as 'Evaluation' first so the database 
-- allows us to insert the submissions and scores without triggering the Archive Guard.
INSERT INTO Events (EventName, Status, TargetDepartment, MinSemester, MaxSemester) VALUES
('National Science Fair 2026', 'Evaluation', 'Computer Science', 4, 8),
('Spring Code Sprint', 'Live', 'Computer Science', 2, 8),
('UI/UX Design Challenge', 'Draft', NULL, NULL, NULL);
GO

-- ======================================================================
-- 5. POPULATE JUDGES 
-- ======================================================================
INSERT INTO EventJudges (EventID, TeacherID) VALUES
(1, 1), 
(1, 2); 
GO

-- ======================================================================
-- 6. POPULATE REGISTRATIONS
-- ======================================================================
INSERT INTO EventRegistrations (EventID, RollNumber, ApprovalStatus) VALUES
(1, '24BSCS01', 'Approved'),
(1, '24BSCS03', 'Approved'),
(1, '24BSCS05', 'Approved');

INSERT INTO EventRegistrations (EventID, RollNumber, ApprovalStatus) VALUES
(2, '24BSCS01', 'Approved');
GO

-- ======================================================================
-- 7. POPULATE SUBMISSIONS
-- ======================================================================
INSERT INTO Submissions (RegID, MaterialLink) VALUES
(1, 'C:\Submissions\ScienceFair\24BSCS01_Project.zip'),
(2, 'C:\Submissions\ScienceFair\24BSCS03_Project.zip'),
(3, 'C:\Submissions\ScienceFair\24BSCS05_Project.zip');
GO

-- ======================================================================
-- 8. POPULATE EVALUATIONS 
-- ======================================================================
INSERT INTO Evaluations (SubmissionID, JudgeAssignmentID, Score, Remarks) VALUES
(1, 1, 88.50, 'Excellent database architecture.'),
(2, 1, 92.00, 'Highly optimized queries, great work.'),
(3, 2, 75.00, 'Good effort, but missing foreign key constraints.');
GO

-- ======================================================================
-- 9. THE ARCHIVE LOCK (State Machine Transition)
-- ======================================================================
-- Now that all data is safely inside, we officially close the event.
-- The Read-Only triggers will now defend this data from any further changes.
UPDATE Events SET Status = 'Closed' WHERE EventID = 1;
GO

PRINT 'Sample data populated successfully. The read-only archive is now locked and active.';