using UniversityEventManagement.DAL;
using UniversityEventManagement.DAL.Repositories;

namespace UniversityEventManagement.BLL;

/// <summary>
/// Business Logic Layer for authentication operations.
/// Handles admin login validation and session management.
/// </summary>
public class AuthService
{
    private readonly AdminRepository _adminRepository;

    public AuthService()
    {
        var dbHelper = new DatabaseHelper();
        _adminRepository = new AdminRepository(dbHelper);
    }

    /// <summary>
    /// Validates user credentials and returns admin details if successful.
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The password (will be hashed for comparison)</param>
    /// <returns>SystemAdmin object if valid, null otherwise</returns>
    public SystemAdmin? Login(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty");
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty");

            return _adminRepository.ValidateCredentials(username, password);
        }
        catch (DataAccessException ex)
        {
            // Log the exception in production
            Console.WriteLine($"Login failed: {ex.Message}");
            throw new BusinessException($"Authentication failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets admin details by ID.
    /// </summary>
    public SystemAdmin? GetAdminById(int adminId)
    {
        try
        {
            return _adminRepository.GetAdminById(adminId);
        }
        catch (DataAccessException ex)
        {
            throw new BusinessException($"Failed to retrieve admin: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Business Logic Layer for dashboard operations.
/// Handles retrieval and processing of dashboard summary data.
/// </summary>
public class DashboardService
{
    private readonly DashboardRepository _dashboardRepository;

    public DashboardService()
    {
        var dbHelper = new DatabaseHelper();
        _dashboardRepository = new DashboardRepository(dbHelper);
    }

    /// <summary>
    /// Retrieves the complete dashboard summary from vw_AdminDashboard_Summary view.
    /// </summary>
    public List<DashboardSummary> GetDashboardSummary()
    {
        try
        {
            return _dashboardRepository.GetDashboardSummary();
        }
        catch (DataAccessException ex)
        {
            throw new BusinessException($"Failed to load dashboard: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves dashboard summary filtered by event status.
    /// </summary>
    public List<DashboardSummary> GetDashboardSummaryByStatus(string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty");

            return _dashboardRepository.GetDashboardSummaryByStatus(status);
        }
        catch (DataAccessException ex)
        {
            throw new BusinessException($"Failed to filter dashboard: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Business Logic Layer for event registration operations.
/// Coordinates registration through stored procedures.
/// </summary>
public class EventRegistrationService
{
    private readonly EventRegistrationRepository _registrationRepository;

    public EventRegistrationService()
    {
        var dbHelper = new DatabaseHelper();
        _registrationRepository = new EventRegistrationRepository(dbHelper);
    }

    /// <summary>
    /// Registers a student for an event.
    /// Business rules are enforced by SQL Triggers (ArchiveGuard) and Stored Procedures.
    /// </summary>
    public int RegisterForEvent(int studentId, int eventId)
    {
        try
        {
            if (studentId <= 0)
                throw new ArgumentException("Invalid student ID");
            
            if (eventId <= 0)
                throw new ArgumentException("Invalid event ID");

            return _registrationRepository.RegisterForEvent(studentId, eventId);
        }
        catch (DataAccessException ex)
        {
            // Graceful exception handling - business rules enforced by DB
            throw new BusinessException($"Registration failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cancels an existing registration.
    /// </summary>
    public void CancelRegistration(int registrationId)
    {
        try
        {
            if (registrationId <= 0)
                throw new ArgumentException("Invalid registration ID");

            _registrationRepository.CancelRegistration(registrationId);
        }
        catch (DataAccessException ex)
        {
            throw new BusinessException($"Cancellation failed: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Custom exception for business logic layer errors.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}
