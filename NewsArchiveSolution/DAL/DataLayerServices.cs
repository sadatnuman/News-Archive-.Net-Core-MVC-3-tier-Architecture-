using DAL.EF;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL
{
    public static class DataLayerServices
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<NewsArchiveDbContext>(opt => opt.UseSqlServer(connectionString));
            services.AddScoped<IDataAccessFactory, DataAccessFactory>();
            return services;
        }

        public static void MigrateDataLayer(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            scope.ServiceProvider.GetRequiredService<NewsArchiveDbContext>().Database.Migrate();
        }
    }
}
