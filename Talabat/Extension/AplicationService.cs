using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entitys.Identity;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Talabat.Dto;
using Talabat.Erorr;
using Talabat.Helper;
using Talabat.Reposatory.Identity;
using Talabat.Reposatory.Repository;
using Talabat.Repository.Repository;

namespace Talabat.Extension
{
    public static class AplicationServiceExtension  // ✅ public static
    {
        public static IServiceCollection AplicationServices(this IServiceCollection services)  // ✅ مش async
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepositry<>));
            // services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddAutoMapper(cfg => { }, typeof(MapeProfile)); // ✅ Syntax صح

            services.AddScoped<IBasketRepository, BasketRepository>();  // ✅ مش typeof لو interface
            services.AddScoped<ITokenService, TokenService>();  // ✅ مش typeof لو interface

            services.AddScoped<IOrderService, Talabat.Services.OrderService.OrderService>();
            // ← جديد
            services.AddScoped<IOrderMapper, OrderMapper>();                                                                                 // Program.cs (before builder.Build())
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToArray();

                    return new BadRequestObjectResult(
                        new ValidationErorrRespose(400, "Validation errors occurred", errors));
                };
            });

            return services;
        }
    }
}