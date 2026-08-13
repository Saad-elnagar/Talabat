using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Talabat.Extension;
using Talabat.Reposatory.Data;
using Talabat.Reposatory.Data.DataSeed;
using Talabat.Reposatory.Identity;

namespace Talabat
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region ConfigServices



            builder.Services.AddDbContext<StoreContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Identity"),
                    b => b.MigrationsAssembly("Talabat")));

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>();


            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                // Get the Redis connection string from configuration
                var connection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"; ;
                return ConnectionMultiplexer.Connect(connection);
            });

            builder.Services.AddHybridCache(options =>
            {

                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10), // Redis Cache expiration time
                    LocalCacheExpiration = TimeSpan.FromMinutes(2) // Local cache expiration time
                };
            });


            builder.Services.AddMemoryCache();

            // NOTE: the "Search" TokenBucketLimiter policy that used to live here was removed —
            // it triggered a known race condition in System.Threading.RateLimiting
            // (SemaphoreFullException) under normal traffic. AddRateLimiter() is kept registered
            // (with no policies) purely because app.UseRateLimiter() further down requires the
            // service to exist in DI.
            builder.Services.AddRateLimiter(options => { });

            // Health checks: hits both SQL Server databases and Redis so /health tells you
            // immediately whether the app can't start because a dependency is down, rather
            // than surfacing as a confusing 500 on some unrelated endpoint later.
            // NOTE: uses small custom IHealthCheck classes (Talabat.HealthChecks) instead of
            // the AspNetCore.HealthChecks.SqlServer/Redis packages — their AddSqlServer/AddRedis
            // extension method signatures have changed across versions and caused a build error;
            // this direct approach has no such version dependency.
            builder.Services.AddHealthChecks()
                .AddCheck(
                    "store-db",
                    new Talabat.HealthChecks.SqlConnectionHealthCheck(builder.Configuration.GetConnectionString("DefaultConnection")!),
                    tags: new[] { "db", "sql", "ready" })
                .AddCheck(
                    "identity-db",
                    new Talabat.HealthChecks.SqlConnectionHealthCheck(builder.Configuration.GetConnectionString("Identity")!),
                    tags: new[] { "db", "sql", "ready" })
                .AddCheck<Talabat.HealthChecks.RedisHealthCheck>(
                    "redis",
                    tags: new[] { "cache", "ready" });


            // 2. تسجيل الـ PaymentIntentService كـ Scoped Service
            //   builder.Services.AddScoped<PaymentIntentService>();


            builder.Services.AplicationServices();



            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            //  JwtBearerDefaults.AuthenticationScheme
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("JwtOnly", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });

            #endregion


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });




            var app = builder.Build();

            #region Database Seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var storeContext = services.GetRequiredService<StoreContext>();
                    await storeContext.Database.MigrateAsync();
                    await StoreContextSeed.SeedAsync(storeContext);

                    var identityContext = services.GetRequiredService<AppIdentityDbContext>();
                    await identityContext.Database.MigrateAsync();

                    var userManager = services.GetRequiredService<UserManager<User>>();
                    await IdentityDbContextSeed.SeedUserAsync(userManager);

                    logger.LogInformation("Database seeded successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            #endregion

            #region Middleware Pipeline
            if (app.Environment.IsDevelopment())
            {

                app.MapOpenApi();
                app.MapScalarApiReference();

            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // CORS must come right after UseRouting and BEFORE UseAuthentication/UseAuthorization —
            // otherwise the browser's CORS preflight requests never reach the CORS middleware.
            app.UseRouting();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapControllers();

            // GET /health        -> overall status (all checks: DB + Redis)
            // GET /health/ready  -> only "ready"-tagged checks, for load balancer / readiness probes
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            #endregion

            await app.RunAsync();
        }
    }
}
