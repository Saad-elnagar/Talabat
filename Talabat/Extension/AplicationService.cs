using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entitys.Identity;
using Talabat.Core.Reposatory;
using Talabat.Erorr;
using Talabat.Helper;
using Talabat.Reposatory.Identity;
using Talabat.Reposatory.Repository;
using Talabat.Repository.Repository;

namespace Talabat.Extension
{
    class AplicationServiceExtension
    {
        public static async Task<IServiceCollection> AplicationServicesAsync(IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepositry<>));
            //builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(cfg => { }, typeof(MapeProfile));
            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

 
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