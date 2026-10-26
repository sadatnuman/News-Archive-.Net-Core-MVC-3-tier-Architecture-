using BLL;
using BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using NewsArchive;
using NewsArchive.Auth;
using NewsArchive.Media;
using NewsArchive.Health;
using NewsArchive.Publishing;
using BLL.Media;
using NewsArchive.Seo;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();
var secureCookies = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = secureCookies;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "Meridian.Session";
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LoginThrottle>();
builder.Services.AddSingleton<IFileStorage, WebRootFileStorage>();
builder.Services.AddHostedService<ScheduledPublisher>();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "Meridian.Antiforgery";
    options.Cookie.SecurePolicy = secureCookies;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "Meridian.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = secureCookies;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var userId = context.Principal?.UserId();
                var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
                var current = userId.HasValue ? authService.GetUser(userId.Value) : null;
                if (context.Principal == null || !context.Principal.MatchesCurrent(current))
                {
                    context.RejectPrincipal();
                    context.HttpContext.Session.Clear();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
                else if (context.Principal.NeedsRefresh(current!))
                {
                    context.ReplacePrincipal(AuthClaims.CreatePrincipal(current!));
                    context.ShouldRenew = true;
                }
            }
        };
    });

var apiOptions = builder.Configuration.GetSection("Api").Get<ApiOptions>() ?? new ApiOptions();
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));

builder.Services.AddCors(options => options.AddPolicy(ApiOptions.CorsPolicy, policy =>
{
    if (apiOptions.AllowedOrigins.Length > 0)
        policy.WithOrigins(apiOptions.AllowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        context.Request.Path.StartsWithSegments("/api")
            ? RateLimitPartition.GetFixedWindowLimiter(
                "api:" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })
            : RateLimitPartition.GetNoLimiter("web"));
    options.AddPolicy(ApiOptions.CommentRateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 6, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy(ApiOptions.LoginRateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The Meridian Newsfolio API",
        Version = "v1",
        Description = "Public news archive endpoints plus token protected author and admin endpoints."
    });
    options.DocInclusionPredicate((_, api) => api.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Paste the token returned by POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
})
.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource));
});

var supportedCultures = AppLanguages.All.Select(code => new CultureInfo(code)).ToList();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(AppLanguages.English);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider { QueryStringKey = UiLanguage.QueryKey, UIQueryStringKey = UiLanguage.QueryKey },
        new CookieRequestCultureProvider()
    };
});

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 1;
    foreach (var proxy in builder.Configuration.GetSection("Hosting:KnownProxies").Get<string[]>() ?? [])
    {
        if (System.Net.IPAddress.TryParse(proxy, out var address))
            options.KnownProxies.Add(address);
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.AddScoped<SeoUrls>();

var connectionString = builder.Configuration.GetConnectionString("DbConn")
    ?? throw new InvalidOperationException("Connection string 'DbConn' is not configured.");
builder.Services.AddBusinessLayer(connectionString);

var app = builder.Build();

var migrateOnStartup = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup");
try
{
    app.Services.InitializeBusinessLayer(migrateOnStartup);
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "The database could not be prepared. Check ConnectionStrings:DbConn in appsettings.json and make sure SQL Server is running.");
    if (!app.Environment.IsDevelopment()) throw;
}

app.UseForwardedHeaders();

static bool IsApiRequest(HttpContext context) => context.Request.Path.StartsWithSegments("/api");

app.UseWhen(IsApiRequest, api =>
{
    api.UseExceptionHandler();
    api.UseStatusCodePages();
});

app.UseWhen(context => !IsApiRequest(context), web =>
{
    if (!app.Environment.IsDevelopment())
        web.UseExceptionHandler("/error/500");
    web.UseStatusCodePagesWithReExecute("/error/{0}");
});

if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseResponseCompression();

var contentSecurityPolicy = string.Join("; ", new[]
{
    "default-src 'self'",
    "base-uri 'self'",
    "object-src 'none'",
    "frame-ancestors 'self'",
    "form-action 'self'",
    "img-src 'self' data: https:",
    "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net",
    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net",
    "script-src 'self'",
    "connect-src 'self'",
    "manifest-src 'self'"
}) + (isDevelopment ? string.Empty : "; upgrade-insecure-requests");

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["X-Frame-Options"] = "SAMEORIGIN";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
    headers["Cross-Origin-Opener-Policy"] = "same-origin";
    if (!context.Request.Path.StartsWithSegments("/swagger"))
        headers["Content-Security-Policy"] = contentSecurityPolicy;
    if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/Auth") ||
        context.Request.Path.StartsWithSegments("/Admin") || context.Request.Path.StartsWithSegments("/Member"))
        headers.CacheControl = "no-store";
    await next();
});

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment() || apiOptions.SwaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "The Meridian Newsfolio API v1");
        options.DocumentTitle = "The Meridian Newsfolio API";
    });
}
app.UseStaticFiles();
app.UseRequestLocalization();

app.Use(async (context, next) =>
{
    var requested = context.Request.Query[UiLanguage.QueryKey].ToString();
    if (AppLanguages.All.Contains(requested))
    {
        context.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(requested)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax, HttpOnly = true });
    }
    await next();
});

app.UseRouting();

app.UseCors();

app.UseRateLimiter();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Article}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
