using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RangersPlayoffStatusChecker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RangersService _rangersService;

        public Worker(ILogger<Worker> logger, RangersService rangersService)
        {
            _logger = logger;
            _rangersService = rangersService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var record = await _rangersService.GetRangersRecord();

                _logger.LogInformation(
                    "[{time}] Points: {points}, Playoff Bound: {playoffBound}", 
                    DateTimeOffset.Now, record.Points, record.IsPlayoffBound());

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
