using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace RangersPlayoffStatusChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var temp = Path.GetTempPath();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File("rangers-log.txt")
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting up the service");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "There was a problem starting the service");
            }
            finally
            {
                Log.Information("Stopping the service");

                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<RangersService>();
                })
                .UseSerilog()
                .UseWindowsService()
                .UseSystemd();
    }
}
