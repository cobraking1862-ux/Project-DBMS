using Microsoft.Data.SqlClient;
using UniversityEventManagement.DAL;

namespace UniversityEventManagement.DAL.Repositories;

/// <summary>
/// Repository for system administrator data access operations.
/// Handles authentication against the SystemAdmins table.
/// </summary>
public class AdminRepository
{
    private readonly DatabaseHelper _dbHelper;

    public AdminRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    /// <summary>
    /// Validates admin credentials against the SystemAdmins table.
    /// Returns the admin details if successful, null otherwise.
    /// </summary>
    public SystemAdmin? ValidateCredentials(string username, string password)
    {
        var parameters = new SqlParameter[]
        {
            new SqlParameter("@Username", username),
            new SqlParameter("@Password", password)
        };

        try
        {
            // Query to validate credentials against SystemAdmins table
            // Assumes there's a PasswordHash column in the table
            var query = @"
                SELECT AdminId, Username, Email, FullName, CreatedDate 
                FROM SystemAdmins 
                WHERE Username = @Username AND PasswordHash = HASHBYTES('SHA2_256', @Password)";

            var dataTable = _dbHelper.ExecuteQuery(query, parameters);

            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                return new SystemAdmin
                {
                    AdminId = Convert.ToInt32(row["AdminId"]),
                    Username = row["Username"].ToString()!,
                    Email = row["Email"].ToString()!,
                    FullName = row["FullName"].ToString()!,
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                };
            }

            return null;
        }
        catch (DataAccessException)
        {
            // Re-throw to be handled by BLL
            throw;
        }
    }

    /// <summary>
    /// Gets admin details by ID.
    /// </summary>
    public SystemAdmin? GetAdminById(int adminId)
    {
        var parameters = new SqlParameter[]
        {
            new SqlParameter("@AdminId", adminId)
        };

        var query = @"
            SELECT AdminId, Username, Email, FullName, CreatedDate 
            FROM SystemAdmins 
            WHERE AdminId = @AdminId";

        var dataTable = _dbHelper.ExecuteQuery(query, parameters);

        if (dataTable.Rows.Count > 0)
        {
            var row = dataTable.Rows[0];
            return new SystemAdmin
            {
                AdminId = Convert.ToInt32(row["AdminId"]),
                Username = row["Username"].ToString()!,
                Email = row["Email"].ToString()!,
                FullName = row["FullName"].ToString()!,
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        return null;
    }
}
