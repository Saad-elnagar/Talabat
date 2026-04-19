
using Microsoft.EntityFrameworkCore;
using Talabat.Reposatory.Data.Config;

namespace Talabat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region ConfigServices

            builder.Services.AddDbContext<StoreContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<StoreContext>();


                context.Database.Migrate(); // This will apply any pending migrations to the database. Make sure you have created migrations using 'dotnet ef migrations add MigrationName' before running the application.
               // await StoreContextSeed.SeedAsync(context); // This will seed the database with initial data if necessary.
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
          //  builder.Services.AddScoped(typeof(IGenenaricRepositry<>), typeof(GenericRepositry<>));

          
            var app = builder.Build(); 
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
