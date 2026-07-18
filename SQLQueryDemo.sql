-- ======================================================================
-- SCRIPT: Exhibition_Setup_Standard.sql
-- PURPOSE: Populates the 3 standard demo events (Closed, Live, Draft)
-- ======================================================================

USE EventManagementDB;
GO

-- 1. INSERT EVENTS
INSERT INTO Events (EventName, Status, TargetDepartment, MinSemester, MaxSemester) VALUES
('National Science Fair 2026', 'Evaluation', 'Computer Science', 4, 8),
('Spring Code Sprint', 'Live', 'Computer Science', 2, 8),
('UI/UX Design Challenge', 'Draft', NULL, NULL, NULL);

-- 2. ASSIGN JUDGES (For the Science Fair)
INSERT INTO EventJudges (EventID, TeacherID) VALUES (1, 1), (1, 2); 

-- 3. REGISTER STUDENTS
-- RegID 1, 2, 3 -> National Science Fair
-- RegID 4 -> Spring Code Sprint
INSERT INTO EventRegistrations (EventID, RollNumber, ApprovalStatus) VALUES
(1, '24BSCS01', 'Approved'),
(1, '24BSCS03', 'Approved'),
(1, '24BSCS05', 'Approved'),
(2, '24BSCS01', 'Approved');

-- 4. ADD SUBMISSIONS (For the Science Fair)
INSERT INTO Submissions (RegID, MaterialLink) VALUES
(1, 'C:\Submissions\ScienceFair\24BSCS01_Project.zip'),
(2, 'C:\Submissions\ScienceFair\24BSCS03_Project.zip'),
(3, 'C:\Submissions\ScienceFair\24BSCS05_Project.zip');

-- 5. ADD EVALUATIONS (For the Science Fair)
INSERT INTO Evaluations (SubmissionID, JudgeAssignmentID, Score, Remarks) VALUES
(1, 1, 88.50, 'Excellent database architecture.'),
(2, 1, 92.00, 'Highly optimized queries, great work.'),
(3, 2, 75.00, 'Good effort, but missing foreign key constraints.');

-- 6. ARCHIVE THE EVENT
-- Transition the status to 'Closed' to activate the Read-Only Archive Guard
UPDATE Events SET Status = 'Closed' WHERE EventID = 1;

PRINT 'Standard demo environment seeded successfully.';
GO