using System.Data.SqlClient;
using UniversityEventManagement.DAL;

namespace UniversityEventManagement.DAL.Repositories;

/// <summary>
/// Repository for event registration operations.
/// Calls the sp_RegisterForEvent stored procedure as per business logic requirements.
/// </summary>
public class EventRegistrationRepository
{
    private readonly DatabaseHelper _dbHelper;

    public EventRegistrationRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    /// <summary>
    /// Registers a student for an event by calling the sp_RegisterForEvent stored procedure.
    /// The stored procedure handles all business rules via SQL Triggers (ArchiveGuard).
    /// </summary>
    /// <param name="studentId">The ID of the student registering</param>
    /// <param name="eventId">The ID of the event</param>
    /// <returns>The registration ID if successful</returns>
    public int RegisterForEvent(int studentId, int eventId)
    {
        var parameters = new SqlParameter[]
        {
            new SqlParameter("@StudentId", studentId),
            new SqlParameter("@EventId", eventId),
            new SqlParameter("@RegistrationId", SqlDbType.Int) { Direction = ParameterDirection.Output }
        };

        try
        {
            _dbHelper.ExecuteStoredProcedureNonQuery("sp_RegisterForEvent", parameters);
            return Convert.ToInt32(parameters[2].Value);
        }
        catch (DataAccessException ex)
        {
            // Re-throw to be handled by BLL with graceful exception handling
            throw new DataAccessException($"Registration failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cancels a registration by calling the appropriate stored procedure.
    /// </summary>
    public void CancelRegistration(int registrationId)
    {
        var parameters = new SqlParameter[]
        {
            new SqlParameter("@RegistrationId", registrationId)
        };

        try
        {
            _dbHelper.ExecuteStoredProcedureNonQuery("sp_CancelRegistration", parameters);
        }
        catch (DataAccessException ex)
        {
            throw new DataAccessException($"Cancellation failed: {ex.Message}", ex);
        }
    }
}
