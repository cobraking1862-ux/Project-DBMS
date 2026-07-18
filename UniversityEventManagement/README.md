# University Event & Competition Management System

A modern WPF Desktop application built with .NET 8 for managing university events and competitions. This application follows a strict 3-Tier Architecture (UI → BLL → DAL) using ADO.NET for database access.

## 📋 Project Structure

```
UniversityEventManagement/
├── UniversityEventManagement.sln          # Visual Studio Solution
├── README.md                               # This file
└── src/
    ├── UniversityEventManagement.DAL/      # Data Access Layer
    │   ├── UniversityEventManagement.DAL.csproj
    │   ├── ConnectionStringConfig.cs       # Database connection string
    │   ├── DatabaseHelper.cs               # ADO.NET helper class
    │   ├── SystemAdmin.cs                  # Admin entity model
    │   ├── DashboardSummary.cs             # Dashboard view model
    │   └── Repositories/
    │       ├── AdminRepository.cs          # Admin authentication
    │       ├── DashboardRepository.cs      # Dashboard data retrieval
    │       └── EventRegistrationRepository.cs  # Event registration
    │
    ├── UniversityEventManagement.BLL/      # Business Logic Layer
    │   ├── UniversityEventManagement.BLL.csproj
    │   └── Services.cs                     # AuthService, DashboardService, EventRegistrationService
    │
    └── UniversityEventManagement.UI/       # Presentation Layer (WPF)
        ├── UniversityEventManagement.UI.csproj
        ├── App.xaml                        # Application resources & styles
        ├── App.xaml.cs
        └── Views/
            ├── LoginWindow.xaml            # Login UI
            ├── LoginWindow.xaml.cs
            ├── MainWindow.xaml             # Main Dashboard UI
            └── MainWindow.xaml.cs
```

## 🏗️ Architecture Overview

### 3-Tier Architecture

1. **Presentation Layer (UI)** - WPF Windows
   - `LoginWindow`: Administrator authentication
   - `MainWindow`: Dashboard displaying `vw_AdminDashboard_Summary`

2. **Business Logic Layer (BLL)** - Service Classes
   - `AuthService`: Handles login validation
   - `DashboardService`: Manages dashboard data operations
   - `EventRegistrationService`: Coordinates event registrations

3. **Data Access Layer (DAL)** - Repository Pattern
   - `DatabaseHelper`: Core ADO.NET operations
   - `AdminRepository`: SystemAdmins table access
   - `DashboardRepository`: View data retrieval
   - `EventRegistrationRepository`: Stored procedure calls

## 🔧 Technical Specifications

### Database Connection String
```
Server=Kernel-Nexus\SQLEXPRESS;Database=EventManagementDB;Integrated Security=True;TrustServerCertificate=True;
```

### Key Technologies
- **.NET 8** - Latest LTS framework
- **WPF** - Windows Presentation Foundation
- **ADO.NET** - Direct database access (No Entity Framework)
- **System.Data.SqlClient** - SQL Server connectivity
- **XAML** - Modern UI design with Material-inspired styling

### Database Schema Context
The application is designed to work with the following database objects:

**Master Tables:**
- `Students` - Student information
- `Teachers` - Faculty/staff information  
- `SystemAdmins` - Administrator accounts (with PasswordHash column)

**Transactional Tables:**
- `Events` - Event definitions
- `EventRegistrations` - Student registrations
- `Submissions` - Competition submissions
- `Evaluations` - Judge evaluations

**Database Objects:**
- `vw_AdminDashboard_Summary` - Aggregated dashboard view
- `sp_RegisterForEvent` - Registration stored procedure
- `ArchiveGuard` - SQL Triggers for business rules

## 🚀 Build Instructions (Visual Studio 2026)

### Prerequisites
1. **Visual Studio 2026** (or Visual Studio 2022 with .NET 8 SDK)
2. **.NET 8 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
3. **SQL Server** with the EventManagementDB database configured
4. **Windows 10/11** (for WPF support)

### Step-by-Step Build Process

#### 1. Open the Solution
```
File → Open → Project/Solution
Navigate to: UniversityEventManagement/UniversityEventManagement.sln
```

#### 2. Restore NuGet Packages
```
Right-click Solution → Restore NuGet Packages
```
Or via Package Manager Console:
```powershell
Update-Package -reinstall
```

#### 3. Configure Database Connection
Ensure your SQL Server instance matches the connection string in `ConnectionStringConfig.cs`:
- Server name: `Kernel-Nexus\SQLEXPRESS` (modify if different)
- Database: `EventManagementDB`

If your server name differs, update line 12 in `ConnectionStringConfig.cs`:
```csharp
public const string ConnectionString = "Server=YOUR_SERVER;Database=EventManagementDB;Integrated Security=True;TrustServerCertificate=True;";
```

#### 4. Build the Solution
```
Build → Build Solution (Ctrl+Shift+B)
```

Expected output in Build pane:
```
========== Build: 3 succeeded, 0 failed ==========
```

#### 5. Run the Application
```
Debug → Start Debugging (F5)
```

### Default Login Credentials
```
Username: admin
Password: admin123
```

> **Note:** The password is hashed using SHA-256 in the database. Ensure the `SystemAdmins` table has a properly hashed password.

## 📝 Database Setup Script (Optional)

If you need to create the database schema, run this script:

```sql
-- Create Database
CREATE DATABASE EventManagementDB;
GO

USE EventManagementDB;
GO

-- SystemAdmins Table
CREATE TABLE SystemAdmins (
    AdminId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARBINARY(64) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Insert default admin (password: admin123, hashed with SHA-256)
INSERT INTO SystemAdmins (Username, PasswordHash, Email, FullName)
VALUES ('admin', HASHBYTES('SHA2_256', 'admin123'), 'admin@university.edu', 'System Administrator');

-- Students Table
CREATE TABLE Students (
    StudentId INT PRIMARY KEY IDENTITY(1,1),
    StudentNumber NVARCHAR(20) UNIQUE NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    EnrollmentDate DATETIME DEFAULT GETDATE()
);

-- Teachers Table
CREATE TABLE Teachers (
    TeacherId INT PRIMARY KEY IDENTITY(1,1),
    EmployeeNumber NVARCHAR(20) UNIQUE NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    Department NVARCHAR(50)
);

-- Events Table
CREATE TABLE Events (
    EventId INT PRIMARY KEY IDENTITY(1,1),
    EventName NVARCHAR(200) NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    EventDate DATETIME NOT NULL,
    Venue NVARCHAR(200),
    OrganizerId INT REFERENCES Teachers(TeacherId),
    Status NVARCHAR(20) DEFAULT 'Upcoming',
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- EventRegistrations Table
CREATE TABLE EventRegistrations (
    RegistrationId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT REFERENCES Students(StudentId),
    EventId INT REFERENCES Events(EventId),
    RegistrationDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Active'
);

-- Submissions Table
CREATE TABLE Submissions (
    SubmissionId INT PRIMARY KEY IDENTITY(1,1),
    RegistrationId INT REFERENCES EventRegistrations(RegistrationId),
    SubmissionDate DATETIME DEFAULT GETDATE(),
    FilePath NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'Pending'
);

-- Evaluations Table
CREATE TABLE Evaluations (
    EvaluationId INT PRIMARY KEY IDENTITY(1,1),
    SubmissionId INT REFERENCES Submissions(SubmissionId),
    JudgeId INT REFERENCES Teachers(TeacherId),
    Score DECIMAL(5,2),
    Comments NVARCHAR(500),
    EvaluationDate DATETIME DEFAULT GETDATE()
);

-- Dashboard View
CREATE VIEW vw_AdminDashboard_Summary AS
SELECT 
    e.EventId,
    e.EventName,
    e.EventType,
    e.EventDate,
    e.Venue,
    COUNT(DISTINCT er.RegistrationId) AS TotalRegistrations,
    COUNT(DISTINCT s.SubmissionId) AS TotalSubmissions,
    COUNT(DISTINCT CASE WHEN ev.EvaluationId IS NULL AND s.SubmissionId IS NOT NULL THEN s.SubmissionId END) AS PendingEvaluations,
    e.Status,
    t.FirstName + ' ' + t.LastName AS OrganizerName
FROM Events e
LEFT JOIN Teachers t ON e.OrganizerId = t.TeacherId
LEFT JOIN EventRegistrations er ON e.EventId = er.EventId
LEFT JOIN Submissions s ON er.RegistrationId = s.RegistrationId
LEFT JOIN Evaluations ev ON s.SubmissionId = ev.SubmissionId
GROUP BY e.EventId, e.EventName, e.EventType, e.EventDate, e.Venue, e.Status, t.FirstName, t.LastName;

-- Stored Procedure for Registration
CREATE PROCEDURE sp_RegisterForEvent
    @StudentId INT,
    @EventId INT,
    @RegistrationId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if already registered
        IF EXISTS (SELECT 1 FROM EventRegistrations WHERE StudentId = @StudentId AND EventId = @EventId)
        BEGIN
            RAISERROR('Student is already registered for this event.', 16, 1);
            RETURN;
        END
        
        -- Insert registration
        INSERT INTO EventRegistrations (StudentId, EventId, RegistrationDate, Status)
        VALUES (@StudentId, @EventId, GETDATE(), 'Active');
        
        SET @RegistrationId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
```

## 🎨 UI Features

### Login Window
- Modern card-based design with gradient header
- Input validation with error messages
- Enter key support for quick login
- Credential hint for testing

### Main Dashboard
- **Top Navigation Bar**: App title, user info, logout button
- **Filter Controls**: Status filter dropdown, refresh button
- **DataGrid**: Displays `vw_AdminDashboard_Summary` with columns:
  - Event ID, Name, Type, Date, Venue
  - Registrations, Submissions, Pending Evaluations
  - Status, Organizer
- **State Management**: Loading, Empty, Error states
- **Responsive Design**: Auto-sizing columns, modern styling

## 🔐 Security Considerations

1. **Password Hashing**: Passwords are hashed using SHA-256 in the database
2. **Parameterized Queries**: All SQL queries use SqlParameter to prevent SQL injection
3. **Exception Handling**: Graceful error handling with user-friendly messages
4. **Connection Security**: TrustServerCertificate enabled for development

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| System.Data.SqlClient | 4.8.6 | SQL Server connectivity |
| .NET 8 Windows | 8.0.x | WPF framework |

## 🛠️ Troubleshooting

### Common Issues

**1. "Login failed" error**
- Verify SQL Server is running
- Check connection string matches your server name
- Ensure `EventManagementDB` database exists
- Verify `SystemAdmins` table has correct credentials

**2. "Cannot load dashboard" error**
- Confirm `vw_AdminDashboard_Summary` view exists
- Check database permissions
- Review SQL Server error logs

**3. Build errors**
- Ensure .NET 8 SDK is installed
- Run `dotnet restore` from solution directory
- Clean and rebuild solution

**4. WPF rendering issues**
- Update graphics drivers
- Ensure Windows updates are current
- Check Hardware Graphics Acceleration settings

## 📄 License

This project is provided as-is for educational and demonstration purposes.

## 👨‍💻 Author

Generated for University Event & Competition Management System requirements.

---

**Built with ❤️ using .NET 8 and WPF**
