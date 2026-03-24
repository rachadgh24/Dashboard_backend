using Microsoft.Data.SqlClient;
using Npgsql;

namespace task1.Application.Resilience;

/// <summary>
/// Determines if an exception represents a transient failure worth retrying.
/// Does NOT include ArgumentException, validation errors, or business logic errors.
/// </summary>
internal static class TransientExceptionPredicate
{
    /// <summary>PostgreSQL transient error codes (connection, deadlock, resource exhaustion).</summary>
    private static readonly HashSet<string> TransientPostgresStates = new(StringComparer.Ordinal)
    {
        "08000", "08003", "08006", "08001", "08004", "08007", // connection
        "40001", "40P01",                                     // serialization/deadlock
        "53000", "53100", "53200",                            // insufficient resources
        "57P01", "57P02", "57P03"                             // admin/crash shutdown, cannot connect
    };

    /// <summary>SQL Server transient error numbers.</summary>
    private static readonly HashSet<int> TransientSqlErrorNumbers = new()
    {
        -2, -1, 64, 233, 4060, 40197, 40501, 40613, 49918, 49919, 49920, 10928, 10929, 40143
    };

    public static bool IsTransient(Exception? ex)
    {
        if (ex == null) return false;
        var current = ex;
        while (current != null)
        {
            if (IsTransientCore(current))
                return true;
            current = current.InnerException;
        }
        return false;
    }

    private static bool IsTransientCore(Exception ex)
    {
        return ex switch
        {
            HttpRequestException => true,
            TimeoutException => true,
            PostgresException pg => TransientPostgresStates.Contains(pg.SqlState),
            NpgsqlException npg when npg.InnerException is IOException or TimeoutException => true,
            SqlException sql => sql.Errors.Cast<SqlError>().Any(e => TransientSqlErrorNumbers.Contains(e.Number)),
            _ => false
        };
    }
}
