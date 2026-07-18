# Document D: AI Coding Assistant Directives (The "AI Rulebook")

## 1. Overview & Purpose
This document serves as the mandatory operational protocol for any AI assistant (including myself) tasked with generating, refactoring, or modifying the codebase for the University Event & Competition Management System. The primary objective of these directives is to prevent "AI hallucinations" or unauthorized architectural shifts that could compromise the integrity of the project during the undergraduate exhibition.

## 2. Core Development Philosophy
The AI must strictly adhere to the following priority hierarchy when suggesting code:

*   **Priority 1: Database-First Logic.** The AI must prioritize utilizing MS SQL Server features (Triggers, Views, Stored Procedures, Constraints) to handle business rules. C# code should be used only for UI-to-Database data transportation and basic validation.
*   **Priority 2: Master vs. Transactional Separation.** The AI is forbidden from generating logic that treats Master Data (Students/Teachers) as transactional. All code must enforce the existing schema where registrations and evaluations link back to pre-existing Master Data records.
*   **Priority 3: Anti-Over-Engineering.** The AI must avoid introducing complex patterns (e.g., microservices, ORMs like Entity Framework, or unnecessary async patterns) that obfuscate the simple, transparent 3-Tier architecture.

## 3. Scope of Modification Constraints

When prompted by the user to modify the application, the AI must categorize the request and adhere to these strict boundaries:

### A. UI/Interface Modifications
*   **Permitted:** Styling (XAML), layout adjustments, adding new WPF controls, or improving UI/UX flow.
*   **Prohibited:** Replacing the established 3-Tier structure, bypassing the DAL, or hardcoding connection logic directly into the UI layer.

### B. Logical & Architectural Modifications
*   **Permitted:** Adjusting existing methods within the BLL or DAL to accommodate new features.
*   **Prohibited:** Modifying core database schemas, deleting triggers, or changing the fundamental state machine (Draft $\rightarrow$ Live $\rightarrow$ Closed) without explicit, multi-step confirmation from the user.

## 4. The Confirmation Protocol
The AI is strictly prohibited from executing "silent" structural changes. Before proceeding, the AI **must** receive explicit approval if a requested change falls into any of the following categories:

1.  **Schema Destabilization:** Any request involving `DROP`, `ALTER`, or `TRUNCATE` commands on any database table.
2.  **Architectural Shift:** Any request to remove the BLL/DAL separation or introduce third-party libraries that replace native ADO.NET.
3.  **Core State Logic:** Any request to alter the Read-Only Archive Guard or Score Audit triggers, as these are critical for the exhibition's demonstration of "Database Integrity."

**The Interaction Workflow:**
*   **User Prompt:** "Make the evaluation screen show the student's name."
*   **AI Assessment:** Does this break blind judging/transparency constraints?
*   **Action:** If yes, the AI must first explain the potential conflict with the project's "blind evaluation" philosophy before generating any code.

## 5. Deployment Context
*   **Local Machine Environment:** The AI must assume all connection strings and environment settings are strictly local (`Kernel-Nexus\SQLEXPRESS`). 
*   **No Cloud/Remote Assumptions:** The AI must not suggest or generate code for cloud-based storage, remote API calls, or server-side hosting unless the user explicitly requests a migration.

---

**Policy Enforcement:** 
By adhering to these rules, the AI assistant ensures that the project remains a high-quality demonstration of DBMS engineering, keeping the C# logic clean and the SQL logic powerful.