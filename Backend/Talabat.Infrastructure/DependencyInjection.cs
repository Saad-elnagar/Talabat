using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Talabat.Application.Abstractions;
using Talabat.Domain.Repositories;
using Talabat.Infrastructure.Identity;
using Talabat.Infrastructure.Persistence;
using Talabat.Infrastructure.Repositories;
using Talabat.Infrastructure.Services.Auth;
using Talabat.Infrastructure.Services.Cache;
using Talabat.Infrastructure.Services.Payments;

namespace Talabat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StoreContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Identity"),
                sql => sql.MigrationsAssembly(typeof(AppIdentityDbContext).Assembly.GetName().Name)));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(connection);
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });
        services.AddMemoryCache();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepositry<>));
        services.AddScoped<IBasketRepository, BasketRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentIntent, PaymentService>();
        services.AddScoped<Stripe.PaymentIntentService>();
        services.AddSingleton<ICacheService, MultiLayerCacheService>();

        return services;
    }
}
