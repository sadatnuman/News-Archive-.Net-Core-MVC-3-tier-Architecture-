using BLL.Mapping;
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
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddScoped<ArticleService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<CommentService>();
            services.AddScoped<TagService>();
            services.AddScoped<UserService>();
            services.AddScoped<RoleService>();
            services.AddScoped<AnalyticsService>();
            services.AddScoped<AuthService>();
            services.AddScoped<MediaService>();
            services.AddScoped<SystemService>();

            return services;
        }

        public static void InitializeBusinessLayer(this IServiceProvider services, bool applyMigrations)
        {
            if (applyMigrations)
                services.MigrateDataLayer();

            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            provider.GetRequiredService<ArticleService>().EnsureSlugs();
            provider.GetRequiredService<CategoryService>().EnsureSlugs();
            provider.GetRequiredService<TagService>().EnsureSlugs();
            provider.GetRequiredService<AuthService>().PurgeExpiredTokens();
        }
    }
}
