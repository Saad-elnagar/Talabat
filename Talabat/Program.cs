using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Talabat.Core.Reposatory;
using Talabat.Helper;
using Talabat.Reposatory.Data.Config;
using Talabat.Reposatory.Data.DataSeed;
using Talabat.Reposatory.Repository;

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



            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<StoreContext>();


                context.Database.Migrate(); // This will apply any pending migrations to the database. Make sure you have created migrations using 'dotnet ef migrations add MigrationName' before running the application.
                await StoreContextSeed.SeedAsync(context); // This will seed the database with initial data if necessary.
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
           builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepositry<>));
            //builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddAutoMapper(cfg => { }, typeof(MapeProfile));

            var app = builder.Build();
            #endregion
                
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
