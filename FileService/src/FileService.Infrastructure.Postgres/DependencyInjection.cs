using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgresEfCore(this IServiceCollection services, IConfiguration configuration)
    {
            var connString =  configuration.GetConnectionString("Postgres");
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException($"Postgres connection string is null or empty");

            services.AddDbContext<FileServiceDbContext>(options =>
            {
                options.UseNpgsql(connString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(FileServiceDbContext).Assembly.FullName);
                    });
                options.EnableDetailedErrors();
            });
            
            return services;
    }
}