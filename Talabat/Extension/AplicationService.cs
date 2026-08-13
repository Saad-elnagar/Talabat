using Talabat.Services.Stripe;

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
            services.AddScoped<IOrderMapper, OrderMapper>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
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