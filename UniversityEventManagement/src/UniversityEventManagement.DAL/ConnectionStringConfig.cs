namespace UniversityEventManagement.DAL;

/// <summary>
/// Centralized configuration for database connectivity.
/// </summary>
public static class ConnectionStringConfig
{
    /// <summary>
    /// The mandatory connection string as specified in requirements.
    /// Server=Kernel-Nexus\\SQLEXPRESS;Database=EventManagementDB;Integrated Security=True;TrustServerCertificate=True;
    /// </summary>
    public const string ConnectionString = "Server=Kernel-Nexus\\SQLEXPRESS;Database=EventManagementDB;Integrated Security=True;TrustServerCertificate=True;";
}
