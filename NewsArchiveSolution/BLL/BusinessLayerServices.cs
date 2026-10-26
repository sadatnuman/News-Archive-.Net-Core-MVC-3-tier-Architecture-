using BLL.Interfaces;
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

            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IBookmarkService, BookmarkService>();
            services.AddScoped<IArticleLookupService, ArticleLookupService>();

            return services;
        }

        public static void InitializeBusinessLayer(this IServiceProvider services, bool applyMigrations)
        {
            if (applyMigrations)
                services.MigrateDataLayer();

            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            provider.GetRequiredService<IArticleService>().EnsureSlugs();
            provider.GetRequiredService<ICategoryService>().EnsureSlugs();
            provider.GetRequiredService<ITagService>().EnsureSlugs();
            provider.GetRequiredService<IAuthService>().PurgeExpiredTokens();
        }
    }
}
