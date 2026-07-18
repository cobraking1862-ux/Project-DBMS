using Microsoft.Data.SqlClient;
using UniversityEventManagement.DAL;

namespace UniversityEventManagement.DAL.Repositories;

/// <summary>
/// Repository for dashboard data access operations.
/// Handles retrieval of data from vw_AdminDashboard_Summary view.
/// </summary>
public class DashboardRepository
{
    private readonly DatabaseHelper _dbHelper;

    public DashboardRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    /// <summary>
    /// Retrieves all dashboard summary data from the vw_AdminDashboard_Summary view.
    /// </summary>
    public List<DashboardSummary> GetDashboardSummary()
    {
        try
        {
            var query = "SELECT * FROM vw_AdminDashboard_Summary ORDER BY EventDate DESC";
            var dataTable = _dbHelper.ExecuteQuery(query);

            var summaries = new List<DashboardSummary>();

            foreach (DataRow row in dataTable.Rows)
            {
                summaries.Add(new DashboardSummary
                {
                    EventId = Convert.ToInt32(row["EventId"]),
                    EventName = row["EventName"].ToString()!,
                    EventType = row["EventType"].ToString()!,
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    Venue = row["Venue"].ToString()!,
                    TotalRegistrations = Convert.ToInt32(row["TotalRegistrations"]),
                    TotalSubmissions = Convert.ToInt32(row["TotalSubmissions"]),
                    PendingEvaluations = Convert.ToInt32(row["PendingEvaluations"]),
                    Status = row["Status"].ToString()!,
                    OrganizerName = row["OrganizerName"].ToString()!
                });
            }

            return summaries;
        }
        catch (DataAccessException)
        {
            // Re-throw to be handled by BLL
            throw;
        }
    }

    /// <summary>
    /// Retrieves dashboard summary filtered by status.
    /// </summary>
    public List<DashboardSummary> GetDashboardSummaryByStatus(string status)
    {
        var parameters = new SqlParameter[]
        {
            new SqlParameter("@Status", status)
        };

        try
        {
            var query = "SELECT * FROM vw_AdminDashboard_Summary WHERE Status = @Status ORDER BY EventDate DESC";
            var dataTable = _dbHelper.ExecuteQuery(query, parameters);

            var summaries = new List<DashboardSummary>();

            foreach (DataRow row in dataTable.Rows)
            {
                summaries.Add(new DashboardSummary
                {
                    EventId = Convert.ToInt32(row["EventId"]),
                    EventName = row["EventName"].ToString()!,
                    EventType = row["EventType"].ToString()!,
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    Venue = row["Venue"].ToString()!,
                    TotalRegistrations = Convert.ToInt32(row["TotalRegistrations"]),
                    TotalSubmissions = Convert.ToInt32(row["TotalSubmissions"]),
                    PendingEvaluations = Convert.ToInt32(row["PendingEvaluations"]),
                    Status = row["Status"].ToString()!,
                    OrganizerName = row["OrganizerName"].ToString()!
                });
            }

            return summaries;
        }
        catch (DataAccessException)
        {
            throw;
        }
    }
}
