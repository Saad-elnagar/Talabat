using Microsoft.Extensions.DependencyInjection;
using Talabat.Application.Abstractions;
using Talabat.Application.Mapping;
using Talabat.Application.Services;

namespace Talabat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(ApplicationMappingProfile));
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderMapper, OrderMapper>();

        return services;
    }
}
