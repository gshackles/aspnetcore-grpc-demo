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
        private static HealthCheckResult _lastHealthCheckResult = HealthCheckResult.Unhealthy();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
                options.Interceptors.Add<TracingInterceptor>());

            services.Configure<HealthCheckPublisherOptions>(options => options.Period = TimeSpan.FromSeconds(5));
            services.AddGrpcHealthChecks()
                .AddCheck("", () => 
                    // cycle through the different statuses in order of Degraded -> Healthy -> Unhealthy -> Degraded
                    _lastHealthCheckResult = _lastHealthCheckResult.Status switch
                    {
                        HealthStatus.Degraded => HealthCheckResult.Healthy(),
                        HealthStatus.Healthy => HealthCheckResult.Unhealthy(),
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