# Document A: Overall Project Plan & Lifecycle Workflow

## 1. System Overview
The **University Event & Competition Management System** is a desktop application designed to streamline the registration, submission, and evaluation processes for university events. The core philosophy of this system relies on robust database architecture rather than frontend logic. It leverages established master data (existing university students and teachers) to automate eligibility, enforce blind judging for transparency, and guarantee historical data integrity through state-machine constraints.

## 2. Core System Goals
*   **Data Integrity over Application Logic:** Enforce business rules, such as preventing overlapping registrations or unauthorized evaluations, directly via SQL Server constraints, triggers, and stored procedures.
*   **Automated Eligibility:** Utilize pre-existing master data (Student Department and Semester) to instantly approve or reject event applications without manual administrative intervention.
*   **Evaluation Transparency:** Guarantee unbiased scoring by ensuring judges (Teachers) are only assigned *after* all submissions are finalized, with students' personal identities hidden during the scoring process.
*   **Historical Immutability:** Enforce a strict read-only archival state for all closed events to protect against subsequent data manipulation.

---

## 3. User Roles & Permissions

The application relies on three distinct authorization levels, interacting strictly with the data scoped to their permissions.

| Role | Core Identity | Capabilities & Permissions |
| :--- | :--- | :--- |
| **Administrator** | Super User | Creates and manages events, configures eligibility rules (Target Department/Semester), manually assigns judges post-submission, and advances the global event state. |
| **Student** | Master Data: `RollNumber` | Views live events, applies for registration (subject to automated database approval), and uploads submission materials. Cannot view other students' submissions or assigned judges. |
| **Judge** | Master Data: `TeacherID` | Views assigned submissions purely by `SubmissionID` (blind evaluation), enters scores, and submits remarks. Cannot access events or submissions they are not explicitly assigned to. |

---

## 4. The Core State Machine (Event Lifecycle)

The fundamental architecture of the system revolves around the strict lifecycle of an `Event`. The SQL database will enforce these states, preventing any out-of-sequence actions.

### Phase 1: Setup (`Status = 'Draft'`)
*   **Trigger Action:** The Administrator creates a new event.
*   **Constraints:** Eligibility rules (Min/Max Semester, Target Department) are defined. No student can register yet.

### Phase 2: Registration (`Status = 'Live'`)
*   **Trigger Action:** The Administrator opens the event.
*   **Constraints:** 
    *   Students attempt to register. 
    *   A Stored Procedure intercepts the application, checking the `Students` master table against the `Events` rule columns. 
    *   Approved students may upload their project materials.

### Phase 3: The Transparency Lock (`Status = 'Submissions Closed'`)
*   **Trigger Action:** The Administrator formally closes the submission window.
*   **Constraints:** 
    *   The database rejects any new `INSERT` commands to the `Registrations` or `Submissions` tables for this event. 
    *   The Administrator assigns specific Teachers to the event as Judges.

### Phase 4: Blind Evaluation (`Status = 'Evaluation'`)
*   **Trigger Action:** The Judges access their dashboards.
*   **Constraints:** 
    *   Judges review materials and insert records into the `Evaluations` table. 
    *   The `ScoreAudits` database trigger monitors for any altered grades, logging the timestamp and previous score to prevent tampering.

### Phase 5: Archival (`Status = 'Closed'`)
*   **Trigger Action:** The Administrator finalizes the event and views the leaderboard.
*   **Constraints:** 
    *   The ultimate **Read-Only Archive Guard Trigger** activates. 
    *   Any `UPDATE`, `INSERT`, or `DELETE` attempt on the event's registrations, submissions, or evaluations is aggressively rejected by the SQL Server engine, ensuring permanent historical integrity.