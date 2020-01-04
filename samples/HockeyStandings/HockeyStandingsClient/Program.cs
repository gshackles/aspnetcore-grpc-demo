using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using HockeyStandings;

namespace HockeyStandingsClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001").Intercept(new TracingInterceptor());

            var client = new HockeyStandings.HockeyStandings.HockeyStandingsClient(channel);

            //await MakeUnaryCalls(client);
            //await MakeServerStreamingCall(client);
            //await MakeBidirectionalStreamingCall(client);
            //await MakeStreamingCallWithCancellation(client);

            Console.ReadLine();
        }

        private static async Task MakeUnaryCalls(HockeyStandings.HockeyStandings.HockeyStandingsClient client)
        {
            Console.WriteLine("Enter team names to look up their records, empty string will exit...");

            string query;

            while ((query = Console.ReadLine()) != "")
            {
                var request = new GetTeamRecordRequest {Query = query};

                var record = await client.GetTeamRecordAsync(request);

                PrintRecord(record);
            }
        }

        private static async Task MakeServerStreamingCall(HockeyStandings.HockeyStandings.HockeyStandingsClient client)
        {
            using var call = client.GetAllTeamRecords(new GetAllTeamRecordsRequest());

            await foreach (var record in call.ResponseStream.ReadAllAsync())
                PrintRecord(record);
        }

        private static async Task MakeBidirectionalStreamingCall(HockeyStandings.HockeyStandings.HockeyStandingsClient client)
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
                await requestStream.WriteAsync(new GetTeamRecordRequest {Query = query});

            await requestStream.CompleteAsync();
            await readResponse;
        }

        private static async Task MakeStreamingCallWithCancellation(HockeyStandings.HockeyStandings.HockeyStandingsClient client)
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
    }
}
