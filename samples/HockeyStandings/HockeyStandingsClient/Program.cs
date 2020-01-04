using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace HockeyStandingsClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001").Intercept(new TracingInterceptor());

            var client = new HockeyStandings.HockeyStandings.HockeyStandingsClient(channel);

            while (true)
            {
                switch (GetMenuChoice())
                {
                    case '1':
                        await Samples.MakeUnaryCalls(client);
                        break;
                    case '2':
                        await Samples.MakeServerStreamingCall(client);
                        break;
                    case '3':
                        await Samples.MakeBidirectionalStreamingCall(client);
                        break;
                    case '4':
                        await Samples.MakeStreamingCallWithCancellation(client);
                        break;
                    case '5':
                        await Samples.MakeHealthChecks(channel);
                        break;
                    case '6':
                        await Samples.MakeServerReflectionCall(channel);
                        break;
                    case 'q':
                    case 'Q':
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }

        private static char GetMenuChoice()
        {
            Console.WriteLine();
            Console.WriteLine("--------------");
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1) Unary");
            Console.WriteLine("2) Server streaming");
            Console.WriteLine("3) Bidirectional streaming");
            Console.WriteLine("4) Streaming with cancellation");
            Console.WriteLine("5) Health checks");
            Console.WriteLine("6) Server reflection");
            Console.WriteLine("q) Quit");
            Console.WriteLine();
            Console.Write("> ");

            var choice = Console.ReadLine()?.First() ?? '0';

            Console.WriteLine();

            return choice;
        }
    }
}
