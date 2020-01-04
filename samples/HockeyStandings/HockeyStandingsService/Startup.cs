using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace HockeyStandingsService
{
    public class Startup
    {
        private static readonly Random _random = new Random();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
                options.Interceptors.Add<TracingInterceptor>());

            services.AddGrpcHealthChecks()
                .AddCheck("", () => _random.Next(1, 3) switch
                {
                    1 => HealthCheckResult.Healthy(),
                    2 => HealthCheckResult.Unhealthy(),
                    _ => HealthCheckResult.Degraded(),
                });

            services.AddGrpcReflection();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Services.HockeyStandingsService>();
                endpoints.MapGrpcHealthChecksService();
                endpoints.MapGrpcReflectionService();
            });
        }
    }
}