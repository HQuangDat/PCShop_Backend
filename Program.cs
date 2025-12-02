using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PCShop_Backend.Data;
using PCShop_Backend.Models;
using PCShop_Backend.Service;
using PCShop_Backend.Middleware;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;
using Hangfire;
using Hangfire.SqlServer;

// Configure Serilog BEFORE creating the WebApplication builder
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        path: "logs/log-.txt",
        restrictedToMinimumLevel: LogEventLevel.Information,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "PCShop_Backend");

// Chỉ thêm Console logging khi debug
#if DEBUG
loggerConfig.WriteTo.Console(
    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
#endif

Log.Logger = loggerConfig.CreateLogger();

try
{
    Log.Information("===============================================");
    Log.Information("Starting up PCShop Backend Service");
    Log.Information("===============================================");

    var builder = WebApplication.CreateBuilder(args);

    // Replace the default logging with Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[]{}
            }
        });
    });

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<ISupportService, SupportService>();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddStackExchangeRedisCache(redisOptions =>
        redisOptions.Configuration = builder.Configuration.GetConnectionString("Redis")
    );

    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
        options.AddPolicy("User", policy => policy.RequireRole("User"));
        options.AddPolicy("Staff", policy => policy.RequireRole("Staff"));
    });

    builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));
    builder.Services.AddHangfireServer();

    var app = builder.Build();

    Log.Information("Application built successfully");

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Running in Development environment");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    app.UseHangfireDashboard();

    app.MapControllers();

    Log.Information("===============================================");
    Log.Information("PCShop Backend Service Started Successfully");
    Log.Information("Listening for requests on configured endpoints...");
    Log.Information("===============================================");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("===============================================");
    Log.Information("PCShop Backend Service Shutting Down");
    Log.Information("===============================================");
    Log.CloseAndFlush();
}