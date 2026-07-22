using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Talabat.Core.Entitys.Identity;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Talabat.Dto;
using Talabat.Erorr;
using Talabat.Helper;
using Talabat.Reposatory.Identity;
using Talabat.Reposatory.Repository;
using Talabat.Repository.Repository;
using Talabat.Services.CachService;
using Talabat.Services.EmailService;
using Talabat.Services.PaymentService;

namespace Talabat.Extension
{
    public static class AplicationServiceExtension
    {
        public static IServiceCollection AplicationServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepositry<>));


            services.AddAutoMapper(cfg => { }, typeof(MapeProfile));
            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddScoped<ITokenService, Core.Service.TokenService>();
            services.AddScoped<IOrderService, Talabat.Services.OrderService.OrderService>();
            services.AddSingleton<ICachService, MultiLayerCacheService>();
            services.AddScoped<IOrderMapper, OrderMapper>();                                                                                 // Program.cs (before builder.Build())
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();
            services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<IStripeCheckoutService, StripeCheckoutService>();
            services.AddScoped<IStripeSessionClient, StripeSessionClient>();

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