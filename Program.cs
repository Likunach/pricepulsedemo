using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.DAOs.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PricePulseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register DAOs for Dependency Injection
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IProfileDAO, ProfileDAO>();
builder.Services.AddScoped<ICompanyProfileDAO, CompanyProfileDAO>();
builder.Services.AddScoped<IAddressDAO, AddressDAO>();
builder.Services.AddScoped<IOwnProductDAO, OwnProductDAO>();
builder.Services.AddScoped<IPriceDAO, PriceDAO>();
builder.Services.AddScoped<ICompetitorDAO, CompetitorDAO>();
builder.Services.AddScoped<ICompetitorProductDAO, CompetitorProductDAO>();
builder.Services.AddScoped<ICompetitorProductAnalysisDAO, CompetitorProductAnalysisDAO>();
builder.Services.AddScoped<IContactDAO, ContactDAO>();
builder.Services.AddScoped<IAuthenticationDAO, AuthenticationDAO>();
builder.Services.AddScoped<ISessionDAO, SessionDAO>();
builder.Services.AddScoped<IUserRoleDAO, UserRoleDAO>();
builder.Services.AddScoped<IRegistrationVerificationDAO, RegistrationVerificationDAO>();

// Register Services
builder.Services.AddScoped<PricePulse.Services.IEmailService, PricePulse.Services.EmailService>();
builder.Services.AddScoped<PricePulse.Services.IRegistrationService, PricePulse.Services.RegistrationService>();

// Configure HttpClient with SSL handling for external APIs
builder.Services.AddHttpClient<PricePulse.Services.IOpenAIService, PricePulse.Services.OpenAIService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
    client.DefaultRequestHeaders.Add("User-Agent", "PricePulse/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    // For development, you might want to ignore SSL certificate errors
    // In production, you should handle SSL certificates properly
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
    }
    // Enable automatic decompression
    handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.Brotli;
    return handler;
})
.AddPolicyHandler(GetRetryPolicy());


builder.Services.AddScoped<PricePulse.Services.ICompetitorProductAnalysisService, PricePulse.Services.CompetitorProductAnalysisService>();
builder.Services.AddMemoryCache();

// Register background services
builder.Services.AddHostedService<PricePulse.Services.CompetitorMonitoringService>();

// Authentication: Cookies + Google + Microsoft
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-google";
    })
    .AddMicrosoftAccount("Microsoft", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-microsoft";
    });

var app = builder.Build();

// Add startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== PRICEPULSE APPLICATION STARTING ===");
Console.WriteLine("=== PRICEPULSE APPLICATION STARTING ===");

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

logger.LogInformation("=== PRICEPULSE APPLICATION STARTED SUCCESSFULLY ===");
Console.WriteLine("=== PRICEPULSE APPLICATION STARTED SUCCESSFULLY ===");

app.Run();

// Retry policy for HTTP clients
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => !msg.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
            });
}
