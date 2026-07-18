# Document B: Technical Architecture & Stack Specification

## 1. Architectural Overview

The application utilizes a strict **3-Tier Architecture** to ensure a clean separation of concerns. This structure forces business logic and database commands out of the presentation layer, fulfilling enterprise-level design requirements for the exhibition.

### Layer 1: Presentation (UI Layer)
*   **Framework:** Windows Presentation Foundation (WPF) on .NET 8.
*   **Language:** C# and XAML.
*   **Role:** Handles user interaction, data binding, and visual state. It relies on MVVM (Model-View-ViewModel) principles where appropriate to bind SQL data directly to UI elements (like DataGrids) without manual control manipulation. 
*   **Constraint:** The UI layer will never execute a SQL query directly.

### Layer 2: Business Logic Layer (BLL)
*   **Language:** C#.
*   **Role:** Acts as the mediator between the UI and the database. It enforces application-level rules, validates input formats, and processes raw data returned from the database into usable C# Objects/Models.
*   **Constraint:** The BLL does not hold database connection strings or execute raw SQL commands.

### Layer 3: Data Access Layer (DAL)
*   **Technology:** Native ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlDataAdapter`).
*   **Role:** The only layer permitted to communicate with SQL Server. It handles connection pooling, executes stored procedures, and maps relational database records to C# models.
*   **Constraint:** **Entity Framework is strictly prohibited.** All queries and database interactions must be explicitly written to demonstrate pure DBMS engineering.

---

## 2. Frontend-Backend Connection Flow

To illustrate the architecture, here is the exact execution flow when an Administrator assigns a Judge to an event:

1.  **User Action:** The Admin clicks the "Assign Judge" button in the WPF UI.
2.  **Data Binding & Transfer:** The WPF UI captures the selected `TeacherID` and `EventID` and passes them to the BLL.
3.  **Validation (BLL):** The BLL checks that the IDs are valid integers and that the event is currently in the correct state (e.g., "Submissions Closed").
4.  **Execution (DAL):** The BLL calls a method in the DAL. The DAL opens a `SqlConnection`, constructs a `SqlCommand` targeting the `sp_AssignJudge` stored procedure, attaches the IDs as parameters, and executes the query.
5.  **Database Engine:** SQL Server receives the request, evaluates foreign key constraints, checks for uniqueness, and inserts the record.
6.  **Return & Update:** SQL Server returns a success integer. The DAL closes the connection and passes the result up to the BLL, which signals the WPF UI to refresh the DataGrid.

---

## 3. Local Environment & Database Configuration

The application is designed to run locally on a developer workstation. All DAL connections will default to the following verified SQL Server Express environment details:

*   **Server / Instance Name:** `Kernel-Nexus\SQLEXPRESS`
*   **Host OS:** Windows 10 Pro for Workstations
*   **Authentication Method:** Windows Authentication (Integrated Security)
*   **Local User Account:** `KERNEL-NEXUS\Kernel_Nexus`
*   **Target Database:** `EventManagementDB` (To be created)

### Default Connection String
All ADO.NET connections within the DAL will utilize this standardized connection string:
```csharp
"Server=Kernel-Nexus\\SQLEXPRESS;Database=EventManagementDB;Integrated Security=True;TrustServerCertificate=True;"