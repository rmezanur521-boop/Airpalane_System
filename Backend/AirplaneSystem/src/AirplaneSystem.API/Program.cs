using AirplaneSystem.API.Filters;
using AirplaneSystem.API.Middleware;
using AirplaneSystem.Application;
using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Infrastructure;
using AirplaneSystem.Infrastructure.ExternalServices.FileStorage;
using AirplaneSystem.Infrastructure.Persistence;
using AirplaneSystem.Infrastructure.Seeds;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ── Infrastructure & Application ─────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ── File Storage (local disk under wwwroot/uploads) ──────────────────────────
var webRootPath = builder.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRootPath))
{
    webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    builder.Environment.WebRootPath = webRootPath;
}
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));

builder.Services.AddSingleton<IFileStorageService>(sp =>
    new LocalFileStorageService(webRootPath, sp.GetRequiredService<ILogger<LocalFileStorageService>>()));

// ── Controllers & JSON ───────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ── API Versioning ────────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AgentOrAdmin", policy => policy.RequireRole("Admin", "Agent"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddSlidingWindowLimiter("global", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = int.Parse(builder.Configuration["RateLimit:GlobalPermitLimit"] ?? "100");
        opt.SegmentsPerWindow = 4;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(int.Parse(builder.Configuration["RateLimit:AuthWindowSeconds"] ?? "900"));
        opt.PermitLimit = int.Parse(builder.Configuration["RateLimit:AuthPermitLimit"] ?? "5");
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "db", "sql" });

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AirplaneSystem API",
        Version = "v1",
        Description = "International Airplane Ticketing System REST API",
        Contact = new OpenApiContact { Name = "Silicon Horizon IT", Email = "admin@airsystem.com" }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: **Bearer {token}**",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Memory Cache & HttpContext ────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddResponseCaching();

// ────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ────────────────────────────────────────────────────────────────────────────

// ── Database Migration & Seed ─────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await context.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(context, logger);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration/seed failed");
    }
}

// ── Middleware Pipeline (ORDER IS CRITICAL) ───────────────────────────────────
app.UseHsts();
app.UseHttpsRedirection();

app.UseStaticFiles(); // serves /uploads/** (airline logos/gallery, profile pictures)

app.UseRouting();
app.UseRateLimiter();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCaching();

app.UseMiddleware<AuditMiddleware>();

// ── Swagger ───────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"AirplaneSystem API {description.GroupName.ToUpperInvariant()}");
    });
}

app.MapControllers().RequireRateLimiting("global");
app.MapHealthChecks("/health");

Log.Information("AirplaneSystem API started on {Environment}", app.Environment.EnvironmentName);

app.Run();