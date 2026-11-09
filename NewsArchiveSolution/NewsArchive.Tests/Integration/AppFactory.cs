using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;

namespace NewsArchive.Tests.Integration
{
    public sealed class AppFactory : WebApplicationFactory<Program>
    {
        public static readonly string Connection = Environment.GetEnvironmentVariable("NEWSARCHIVE_TEST_DB")
            ?? @"Data Source=localhost\SQLEXPRESS;Initial Catalog=NewsArchiveTestDB;Integrated Security=True;TrustServerCertificate=True;";

        public AppFactory()
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DbConn", Connection);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
        }

        public static void Execute(string sql, params (string Name, object Value)[] parameters)
        {
            using var connection = new SqlConnection(Connection);
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            foreach (var (name, value) in parameters)
                command.Parameters.AddWithValue(name, value);
            command.ExecuteNonQuery();
        }
    }

    [CollectionDefinition("app", DisableParallelization = true)]
    public sealed class AppCollection : ICollectionFixture<AppFactory>
    {
    }
}
