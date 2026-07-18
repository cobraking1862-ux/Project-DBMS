namespace UniversityEventManagement.DAL;

/// <summary>
/// Represents a dashboard summary item from vw_AdminDashboard_Summary view.
/// </summary>
public class DashboardSummary
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int TotalRegistrations { get; set; }
    public int TotalSubmissions { get; set; }
    public int PendingEvaluations { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
}
