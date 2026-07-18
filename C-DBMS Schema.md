# Document C: DBMS Schema & Implementation Guide

## 1. Database Architectural Overview

This database is designed strictly in 3rd Normal Form (3NF). It cleanly separates **Master Data** (the university's pre-existing records) from **Transactional Data** (the events and competitions). This design eliminates duplicate data entry and leverages database constraints to enforce business rules natively.

### Architectural Highlights (The "Exhibition Flexes")
*   **Flat Rules Integration:** Eligibility criteria are flattened directly into the `Events` table, allowing automated registration approval without complex rule-table joins.
*   **Soft Deletes:** Every table utilizes an `IsActive` bit. The `DELETE` command is practically banned in the application layer, ensuring relational integrity and historical records are permanently preserved.
*   **Focused Auditing:** A dedicated `ScoreAudits` table and trigger enforce transparency by tracking any changes to a judge's submitted score.

---

## 2. Data Dictionary & Schema Definitions

### Master Data Tables

**Table: `Students`**
*   `RollNumber` (VARCHAR(20), **Primary Key**): The unique university ID.
*   `FullName` (VARCHAR(100), NOT NULL)
*   `Department` (VARCHAR(50), NOT NULL)
*   `Semester` (INT, NOT NULL)
*   `IsActive` (BIT, DEFAULT 1): Used for soft deletion.

**Table: `Teachers`**
*   `TeacherID` (INT IDENTITY(1,1), **Primary Key**)
*   `FullName` (VARCHAR(100), NOT NULL)
*   `Department` (VARCHAR(50), NOT NULL)
*   `Designation` (VARCHAR(50), NOT NULL)
*   `IsActive` (BIT, DEFAULT 1)

### Transactional Data Tables

**Table: `Events`**
*   `EventID` (INT IDENTITY(1,1), **Primary Key**)
*   `EventName` (VARCHAR(100), NOT NULL)
*   `Status` (VARCHAR(30), DEFAULT 'Draft'): Valid states are Draft, Live, Submissions Closed, Evaluation, Closed.
*   `TargetDepartment` (VARCHAR(50), NULL): *[Flat Rule]* Restricts event to a specific department. NULL means open to all.
*   `MinSemester` (INT, NULL): *[Flat Rule]* Minimum semester required.
*   `MaxSemester` (INT, NULL): *[Flat Rule]* Maximum semester allowed.
*   `IsActive` (BIT, DEFAULT 1)

**Table: `EventJudges` (The Assignment Bridge)**
*   `JudgeAssignmentID` (INT IDENTITY(1,1), **Primary Key**)
*   `EventID` (INT, **Foreign Key** $\rightarrow$ Events.EventID)
*   `TeacherID` (INT, **Foreign Key** $\rightarrow$ Teachers.TeacherID)
*   `IsActive` (BIT, DEFAULT 1)
*   *Constraint:* `UNIQUE(EventID, TeacherID)` - Prevents assigning the same teacher twice to the same event.

**Table: `EventRegistrations`**
*   `RegID` (INT IDENTITY(1,1), **Primary Key**)
*   `EventID` (INT, **Foreign Key** $\rightarrow$ Events.EventID)
*   `RollNumber` (VARCHAR(20), **Foreign Key** $\rightarrow$ Students.RollNumber)
*   `ApprovalStatus` (VARCHAR(20), DEFAULT 'Pending')
*   `IsActive` (BIT, DEFAULT 1)
*   *Constraint:* `UNIQUE(EventID, RollNumber)` - Prevents a student from applying multiple times.

**Table: `Submissions`**
*   `SubmissionID` (INT IDENTITY(1,1), **Primary Key**)
*   `RegID` (INT, **Foreign Key** $\rightarrow$ EventRegistrations.RegID)
*   `MaterialLink` (VARCHAR(255), NOT NULL)
*   `SubmissionDate` (DATETIME, DEFAULT GETDATE())
*   `IsActive` (BIT, DEFAULT 1)

**Table: `Evaluations`**
*   `EvalID` (INT IDENTITY(1,1), **Primary Key**)
*   `SubmissionID` (INT, **Foreign Key** $\rightarrow$ Submissions.SubmissionID)
*   `JudgeAssignmentID` (INT, **Foreign Key** $\rightarrow$ EventJudges.JudgeAssignmentID)
*   `Score` (DECIMAL(5,2), NOT NULL)
*   `Remarks` (VARCHAR(500), NULL)
*   `IsActive` (BIT, DEFAULT 1)

**Table: `ScoreAudits`**
*   `AuditID` (INT IDENTITY(1,1), **Primary Key**)
*   `EvalID` (INT, **Foreign Key** $\rightarrow$ Evaluations.EvalID)
*   `OldScore` (DECIMAL(5,2), NOT NULL)
*   `NewScore` (DECIMAL(5,2), NOT NULL)
*   `ChangedDate` (DATETIME, DEFAULT GETDATE())

---

## 3. Server-Side Logic (SQL Programmability)

The intelligence of this application lives in SQL Server, not in the C# WPF frontend.

### 3.1. Automated Registration (Stored Procedure)
**Name:** `sp_RegisterForEvent`
*   **Logic:** Receives `RollNumber` and `EventID`. It queries the `Students` table and the `Events` table.
*   **Execution:** 
    *   If `TargetDepartment` matches the student's department AND the student's semester falls between `MinSemester` and `MaxSemester` (or if the event rules are NULL), it `INSERT`s the record with `ApprovalStatus = 'Approved'`.
    *   If the rules fail, it rejects the `INSERT` and returns an error state to the DAL.

### 3.2. The Read-Only Archive Guard (Trigger)
**Name:** `trg_ArchiveGuard_EventData`
*   **Target Tables:** `EventRegistrations`, `Submissions`, `Evaluations`.
*   **Logic:** Attached as an `AFTER INSERT, UPDATE, DELETE` trigger. It checks the parent `EventID` against the `Events` table.
*   **Execution:** If `Events.Status = 'Closed'`, the trigger executes a `ROLLBACK TRANSACTION` and raises a fatal error: *"Action Failed: The target event is archived and strictly read-only."*

### 3.3. Score Transparency (Trigger)
**Name:** `trg_Audit_EvaluationScores`
*   **Target Table:** `Evaluations`
*   **Logic:** Attached as an `AFTER UPDATE` trigger. 
*   **Execution:** If a judge updates an existing score, the trigger captures the `deleted.Score` (old) and `inserted.Score` (new) and silently inserts them into the `ScoreAudits` table for administrative review.

### 3.4. Administrator Dashboard (View)
**Name:** `vw_AdminDashboard_Summary`
*   **Logic:** A complex `SELECT` statement that utilizes `JOIN` and `GROUP BY` to provide a real-time statistical overview for the WPF UI.
*   **Execution:** Aggregates `Events`, `EventRegistrations`, and `Evaluations` to return: `EventName`, `TotalApplicants`, `ApprovedParticipants`, and `AverageScore`, strictly filtering with `WHERE IsActive = 1`.

---

## 4. Maintenance & Exhibition Preparation

### The `Exhibition_Reset.sql` Script
During the exhibition, the application will be tested repeatedly by different evaluators, resulting in cluttered test data. To ensure a perfect demonstration environment every time, the following script methodology will be used:

```sql
-- Exhibition_Reset.sql 
-- Purpose: Wipes transactional data but preserves the University Master Data.

USE EventManagementDB;
GO

-- 1. Delete transactional data (in order of foreign key hierarchy)
DELETE FROM ScoreAudits;
DELETE FROM Evaluations;
DELETE FROM Submissions;
DELETE FROM EventRegistrations;
DELETE FROM EventJudges;
DELETE FROM Events;

-- 2. Reset Identity seeds back to 1 for clean IDs in the next demo
DBCC CHECKIDENT ('ScoreAudits', RESEED, 0);
DBCC CHECKIDENT ('Evaluations', RESEED, 0);
DBCC CHECKIDENT ('Submissions', RESEED, 0);
DBCC CHECKIDENT ('EventRegistrations', RESEED, 0);
DBCC CHECKIDENT ('EventJudges', RESEED, 0);
DBCC CHECKIDENT ('Events', RESEED, 0);

PRINT 'Database successfully reset for the next exhibition demonstration.';