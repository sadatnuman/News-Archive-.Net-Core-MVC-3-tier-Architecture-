using BLL;
using BLL.Audit;
using BLL.Caching;
using BLL.Email;
using BLL.Interfaces;
using BLL.Media;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.WebEncoders;
using Microsoft.OpenApi;
using NewsArchive;
using NewsArchive.Auth;
using NewsArchive.Caching;
using NewsArchive.Email;
using NewsArchive.Health;
using NewsArchive.Media;
using NewsArchive.Publishing;
using NewsArchive.Seo;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();
var secureCookies = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

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
builder.Services.AddScoped<CookieSessionValidator>();
builder.Services.AddScoped<IActorContext, HttpActorContext>();
builder.Services.AddSingleton<IAppCache, MemoryAppCache>();
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
        options.EventsType = typeof(CookieSessionValidator);
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
    void PerIpPerMinute(string policy, int permits) =>
        options.AddPolicy(policy, context => RateLimitPartition.GetFixedWindowLimiter(
            policy + ":" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    PerIpPerMinute(ApiOptions.CommentRateLimit, 6);
    PerIpPerMinute(ApiOptions.ReportRateLimit, 10);
    PerIpPerMinute(ApiOptions.LoginRateLimit, 20);
    PerIpPerMinute(ApiOptions.NewsletterRateLimit, 5);
});

var emailOptions = builder.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>() ?? new EmailOptions();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
if (emailOptions.UseSmtp)
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, OutboxEmailSender>();
builder.Services.AddScoped<MailComposer>();
builder.Services.AddSingleton<PasswordResetStore>();
builder.Services.AddScoped<PasswordChanger>();

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
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var headers = context.Context.Response.Headers;
        if (context.Context.Request.Path == "/sw.js")
            headers.CacheControl = "no-cache";
        else if (context.Context.Request.Query.ContainsKey("v"))
            headers.CacheControl = "public, max-age=31536000, immutable";
        else if (context.Context.Request.Path.StartsWithSegments("/images") || context.Context.Request.Path.StartsWithSegments("/uploads"))
            headers.CacheControl = "public, max-age=86400";
    }
});
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
