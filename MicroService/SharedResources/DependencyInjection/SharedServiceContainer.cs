using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SharedResources.Middleware;

namespace SharedResources.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>(
            this IServiceCollection services, IConfiguration config, string fileName)
            where TContext : DbContext
        {
            // Add Generic Database Context
            services.AddDbContext<TContext>(opt => opt.UseSqlServer(
                config.GetConnectionString("ConnectionString"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

            // Configure Serilog logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    $"{fileName}-.txt",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day
                )
                .CreateLogger();

            // Add JWT authentication
            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);

            return services;
        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalException>();

            app.UseMiddleware<ListenToOnlyApiGateway>();

            return app;
        }
    }
}
