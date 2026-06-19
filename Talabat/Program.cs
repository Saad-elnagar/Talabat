using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Talabat.Core.Entitys.Identity;
using Talabat.Extension;
using Talabat.Reposatory.Data;
using Talabat.Reposatory.Data.Config;
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

            // DbContexts
            builder.Services.AddDbContext<StoreContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Identity"),
                    b => b.MigrationsAssembly("Talabat")));

            // Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

            // Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var connection = builder.Configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connection);
            });

            // Application Services (Repositories, AutoMapper, etc.)
            builder.Services.AplicationServices();  // ✅ بقى extension method عادي

            // Controllers & Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #endregion

            var app = builder.Build();

            #region Database Seeding

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    // 1. StoreContext
                    var storeContext = services.GetRequiredService<StoreContext>();
                    await storeContext.Database.MigrateAsync();
                    await StoreContextSeed.SeedAsync(storeContext);

                    // 2. IdentityDbContext
                    var identityContext = services.GetRequiredService<AppIdentityDbContext>();
                    await identityContext.Database.MigrateAsync();

                    // 3. Seed Users
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
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            #endregion

            await app.RunAsync();
        }
    }
}