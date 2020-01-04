using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using HockeyStandings;

namespace HockeyStandingsClient
{
    static class Samples
    {
        public static Task MakeUnaryCalls(HockeyStandings.HockeyStandings.HockeyStandingsClient client) =>
            Task.Run(async () =>
            {
                Console.WriteLine("Enter team names to look up their records, empty string will exit...");

                string query;

                while ((query = Console.ReadLine()) != "")
                {
                    var request = new GetTeamRecordRequest { Query = query };

                    var record = await client.GetTeamRecordAsync(request);

                    PrintRecord(record);
                }
            });

        public static async Task MakeServerStreamingCall(HockeyStandings.HockeyStandings.HockeyStandingsClient client)
        {
            using var call = client.GetAllTeamRecords(new GetAllTeamRecordsRequest());

            await foreach (var record in call.ResponseStream.ReadAllAsync())
                PrintRecord(record);
        }

        public static Task MakeBidirectionalStreamingCall(HockeyStandings.HockeyStandings.HockeyStandingsClient client) => Task.Run(async () =>
        {
            Console.WriteLine("Enter team names to look up their records, empty string will execute operation...");

            var queries = new List<string>();
            string line;

            while ((line = Console.ReadLine()) != "")
                queries.Add(line);

            var call = client.GetTeamRecords();

            var readResponse = Task.Run(async () =>
            {
                await foreach (var record in call.ResponseStream.ReadAllAsync())
                    PrintRecord(record);
            });

            var requestStream = call.RequestStream;

            foreach (var query in queries)
                await requestStream.WriteAsync(new GetTeamRecordRequest { Query = query });

            await requestStream.CompleteAsync();
            await readResponse;
        });

        public static async Task MakeStreamingCallWithCancellation(HockeyStandings.HockeyStandings.HockeyStandingsClient client)
        {
            try
            {
                var source = new CancellationTokenSource();
                source.CancelAfter(TimeSpan.FromSeconds(3));

                using var call = client.GetAllTeamRecords(new GetAllTeamRecordsRequest());

                await foreach (var record in call.ResponseStream.ReadAllAsync(source.Token))
                    PrintRecord(record);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine($"Received cancellation: status={ex.Status}, code={ex.StatusCode}");
            }
        }

        private static void PrintRecord(TeamRecord record) =>
            Console.WriteLine($"{record.Team.Name}: {record.Points} points, division rank {record.DivisionRank}");

        public static async Task MakeHealthChecks(CallInvoker channel)
        {
            var client = new Health.HealthClient(GrpcChannel.ForAddress("https://localhost:5001"));

            Console.WriteLine("Observing health checks, press enter to exit");

            var source = new CancellationTokenSource();
            var watch = Task.Run(async () =>
            {
                var call = client.Watch(new HealthCheckRequest(), cancellationToken: source.Token);

                try
                {
                    await foreach (var message in call.ResponseStream.ReadAllAsync(source.Token))
                        Console.WriteLine($"[{DateTime.Now}] {message.Status}");
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) { }
            }, source.Token);

            Console.ReadLine();

            source.Cancel();
            await watch;
        }
    }
}