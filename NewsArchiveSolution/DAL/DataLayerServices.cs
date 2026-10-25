using DAL.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL
{
    public static class DataLayerServices
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<NewsArchiveDbContext>(opt => opt.UseSqlServer(connectionString));
            services.AddScoped<DataAccessFactory>();
            return services;
        }
    }
}
