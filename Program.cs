
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Models;
using PCShop_Backend.Service;
using Serilog;

namespace PCShop_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
            try
            {
                Log.Information("Starting up the service...");
                var builder = WebApplication.CreateBuilder(args);
                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddScoped<IProductService, ProductService>();
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IOrderService, OrderService>();
                builder.Services.AddScoped<ISupportService, SupportService>();
                builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
                builder.Services.AddHttpContextAccessor();

                builder.Services.AddStackExchangeRedisCache(redisOptions =>
                    redisOptions.Configuration = builder.Configuration.GetConnectionString("Redis")
                );

                // Authentication and Authorization can be configured here
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                    //.AddGoogle(options =>
                    //{
                    //    options.ClientId = builder.Configuration.GetSection("GoogleKey:ClientId").Value;
                    //    options.ClientSecret = builder.Configuration.GetSection("GoogleKey:ClientSecret").Value;
                    //})
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Auth/Login";
                        options.LogoutPath = "/Auth/Logout";
                    });

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                    options.AddPolicy("User", policy => policy.RequireRole("User"));
                    options.AddPolicy("Employee", policy => policy.RequireRole("Employee"));
                });

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
