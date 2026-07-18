using System.Data;
using Microsoft.Data.SqlClient;

namespace UniversityEventManagement.DAL;

/// <summary>
/// Core data access helper class using ADO.NET.
/// Provides centralized database operations without Entity Framework.
/// </summary>
public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper()
    {
        _connectionString = ConnectionStringConfig.ConnectionString;
    }

    /// <summary>
    /// Executes a query and returns a DataTable.
    /// </summary>
    public DataTable ExecuteQuery(string query, SqlParameter[]? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.CommandType = CommandType.Text;

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        var adapter = new SqlDataAdapter(command);
        var dataTable = new DataTable();
        
        try
        {
            connection.Open();
            adapter.Fill(dataTable);
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Database query failed: {ex.Message}", ex);
        }

        return dataTable;
    }

    /// <summary>
    /// Executes a stored procedure and returns a DataTable.
    /// </summary>
    public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[]? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection);
        command.CommandType = CommandType.StoredProcedure;

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        var adapter = new SqlDataAdapter(command);
        var dataTable = new DataTable();
        
        try
        {
            connection.Open();
            adapter.Fill(dataTable);
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Stored procedure execution failed: {ex.Message}", ex);
        }

        return dataTable;
    }

    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns affected rows.
    /// </summary>
    public int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.CommandType = CommandType.Text;

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        try
        {
            connection.Open();
            return command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Database update failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executes a stored procedure for non-query operations.
    /// </summary>
    public int ExecuteStoredProcedureNonQuery(string procedureName, SqlParameter[]? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection);
        command.CommandType = CommandType.StoredProcedure;

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        try
        {
            connection.Open();
            return command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Stored procedure execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executes a scalar query and returns the result.
    /// </summary>
    public object? ExecuteScalar(string query, SqlParameter[]? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.CommandType = CommandType.Text;

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        try
        {
            connection.Open();
            return command.ExecuteScalar();
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Scalar query failed: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Custom exception for data access layer errors.
/// </summary>
public class DataAccessException : Exception
{
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
}
