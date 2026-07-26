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

            builder.Services.AddRateLimiter(options =>
            {
                options.AddTokenBucketLimiter("Search", searchOptions =>
                {
                    searchOptions.TokenLimit = 20;
                    searchOptions.TokensPerPeriod = 10;
                    searchOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(10);

                });

            });


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

            }
            app.MapScalarApiReference();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapControllers();
            #endregion

            await app.RunAsync();
        }
    }
}