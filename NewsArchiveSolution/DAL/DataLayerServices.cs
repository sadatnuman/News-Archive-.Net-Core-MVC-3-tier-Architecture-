using DAL.EF;
using DAL.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL
{
    public static class DataLayerServices
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<NewsArchiveDbContext>(opt => opt.UseSqlServer(connectionString));

            services.AddScoped<ArticleRepo>();
            services.AddScoped<CategoryRepo>();
            services.AddScoped<CommentRepo>();
            services.AddScoped<TagRepo>();
            services.AddScoped<UserRepo>();
            services.AddScoped<RoleRepo>();

            return services;
        }
    }
}
