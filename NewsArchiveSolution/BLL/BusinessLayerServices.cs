using BLL.Services;
using DAL;
using Microsoft.Extensions.DependencyInjection;

namespace BLL
{
    public static class BusinessLayerServices
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDataLayer(connectionString);

            services.AddScoped<ArticleService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<CommentService>();
            services.AddScoped<TagService>();
            services.AddScoped<UserService>();
            services.AddScoped<RoleService>();
            services.AddScoped<AnalyticsService>();

            return services;
        }
    }
}
