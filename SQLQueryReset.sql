-- ======================================================================
-- SCRIPT: Exhibition_Reset_Transactional.sql
-- PURPOSE: Clears only event-related data, preserving Students & Teachers
-- ======================================================================

USE EventManagementDB;
GO

-- 1. Delete transactional data in reverse order of foreign key dependency
DELETE FROM ScoreAudits;
DELETE FROM Evaluations;
DELETE FROM Submissions;
DELETE FROM EventRegistrations;
DELETE FROM EventJudges;
DELETE FROM Events; 

-- 2. Reset Identity seeds for transactional tables only
DBCC CHECKIDENT ('ScoreAudits', RESEED, 0);
DBCC CHECKIDENT ('Evaluations', RESEED, 0);
DBCC CHECKIDENT ('Submissions', RESEED, 0);
DBCC CHECKIDENT ('EventRegistrations', RESEED, 0);
DBCC CHECKIDENT ('EventJudges', RESEED, 0);
DBCC CHECKIDENT ('Events', RESEED, 0);

PRINT 'Transactional data reset. Master Data (Students/Teachers) remains intact.';
GO