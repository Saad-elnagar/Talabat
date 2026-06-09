using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Reposatory;
using Talabat.Erorr;
using Talabat.Helper;
using Talabat.Reposatory.Repository;

namespace Talabat.Extension
{
    class AplicationServiceExtension
    {
        public static IServiceCollection AplicationServices(IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepositry<>));
            //builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(cfg => { }, typeof(MapeProfile));


            services.Configure<ApiBehaviorOptions>(Options =>
            {

                Options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage).ToArray();

                    return new BadRequestObjectResult(new ValidationErorrRespose(400, "Validation errors occurred", errors));
                };


            });

            return services;
        }
    }
}