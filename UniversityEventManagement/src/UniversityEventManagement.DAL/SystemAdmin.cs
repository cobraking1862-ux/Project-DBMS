namespace UniversityEventManagement.DAL;

/// <summary>
/// Represents a system administrator entity.
/// </summary>
public class SystemAdmin
{
    public int AdminId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
