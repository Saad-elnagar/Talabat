using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Talabat.Api.Errors;
using Talabat.Application;
using Talabat.Domain.Entities.Identity;
using Talabat.Infrastructure;
using Talabat.Infrastructure.Identity;
using Talabat.Infrastructure.Persistence;
using Talabat.Infrastructure.Persistence.DataSeed;

namespace Talabat.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AppIdentityDbContext>();

        builder.Services.AddRateLimiter(options =>
        {
            options.AddTokenBucketLimiter("Search", searchOptions =>
            {
                searchOptions.TokenLimit = 20;
                searchOptions.TokensPerPeriod = 10;
                searchOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            });
        });

        builder.Services.AddControllers();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                var errors = actionContext.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors)
                    .Select(error => error.ErrorMessage)
                    .ToArray();

                return new BadRequestObjectResult(
                    new ValidationErorrRespose(400, "Validation errors occurred", errors));
            };
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
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

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("JwtOnly", policy =>
            {
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        });

        var app = builder.Build();

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
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while seeding the database.");
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapScalarApiReference();
        app.MapGet("/", () => Results.Redirect("/scalar/"));
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();

        await app.RunAsync();
    }
}
