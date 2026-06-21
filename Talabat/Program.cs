using Microsoft.AspNetCore.Authentication.JwtBearer;  // ← جديد
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Talabat.Core.Entitys.Identity;
using Talabat.Core.Service;  // ← عشان ITokenService
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
                var connection = builder.Configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connection);
            });

            builder.Services.AplicationServices();  // ⚠️ تأكد إن الاسم صح (Application مش Aplication)

            builder.Services.AddScoped<ITokenService, TokenService>();  // ← جديد

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ← حطيت JwtBearerDefaults.AuthenticationScheme
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
                            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
                    };
                });

            builder.Services.AddAuthorization();  

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
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();           // ← جديد
            app.UseAuthentication();    // ← بعد Routing
            app.UseAuthorization();     // ← بعد Authentication

            app.MapControllers();       // ← في الآخر
            #endregion

            await app.RunAsync();
        }
    }
}