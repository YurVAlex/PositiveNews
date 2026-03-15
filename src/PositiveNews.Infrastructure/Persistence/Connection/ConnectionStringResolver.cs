using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var defaultConn = configuration.GetConnectionString("DefaultConnection");
        var altConn = configuration.GetConnectionString("AlternativeConnection");

        if (CanConnectToServer(defaultConn))
            return defaultConn!;

        if (CanConnectToServer(altConn))
            return altConn!;

        throw new InvalidOperationException(
            "Unable to connect to either DefaultConnection or AlternativeConnection.");
    }

    private static bool CanConnectToServer(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}