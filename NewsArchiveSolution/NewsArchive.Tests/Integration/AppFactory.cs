using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

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

        public const string IpHeader = "X-Test-Ip";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services => services.AddSingleton<IStartupFilter, ClientIpFilter>());
        }

        private sealed class ClientIpFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                app.Use((context, call) =>
                {
                    if (IPAddress.TryParse(context.Request.Headers[IpHeader].ToString(), out var ip))
                        context.Connection.RemoteIpAddress = ip;
                    return call(context);
                });
                next(app);
            };
        }

        public static object? Scalar(string sql, params (string Name, object Value)[] parameters)
        {
            using var connection = new SqlConnection(Connection);
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            foreach (var (name, value) in parameters)
                command.Parameters.AddWithValue(name, value);
            return command.ExecuteScalar();
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
