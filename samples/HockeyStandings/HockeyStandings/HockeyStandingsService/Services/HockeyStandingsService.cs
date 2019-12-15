using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using HockeyStandings;

namespace HockeyStandingsService.Services
{
    public class HockeyStandingsService : HockeyStandings.HockeyStandings.HockeyStandingsBase
    {
        private readonly NhlService _nhlService;

        public HockeyStandingsService(NhlService nhlService) => 
            _nhlService = nhlService;

        public override async Task<HockeyStandings.TeamRecord> GetTeamRecord(GetTeamRecordRequest request, ServerCallContext context)
        {
            var record = (await _nhlService.GetTeamRecords())
                .FirstOrDefault(teamRecord => teamRecord.Team.Name.ToLowerInvariant().Contains(request.Query.ToLowerInvariant()));

            if (record == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Team not found"));

            return ToTeamRecordMessage(record);
        }

        public override async Task GetAllTeamRecords(GetAllTeamRecordsRequest request, IServerStreamWriter<HockeyStandings.TeamRecord> responseStream, ServerCallContext context)
        {
            var records = await _nhlService.GetTeamRecords();

            foreach (var record in records)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;

                await responseStream.WriteAsync(ToTeamRecordMessage(record));
                await Task.Delay(300);
            }
        }

        public override async Task GetTeamRecords(IAsyncStreamReader<GetTeamRecordRequest> requestStream, IServerStreamWriter<HockeyStandings.TeamRecord> responseStream, ServerCallContext context)
        {
            var records = await _nhlService.GetTeamRecords();

            await foreach (var request in requestStream.ReadAllAsync())
            {
                var record = records.FirstOrDefault(teamRecord => teamRecord.Team.Name.ToLowerInvariant().Contains(request.Query.ToLowerInvariant()));

                if (record != null)
                {
                    await responseStream.WriteAsync(ToTeamRecordMessage(record));
                    await Task.Delay(1000);
                }
            }
        }

        private static HockeyStandings.Team ToTeamMessage(Team team) => new HockeyStandings.Team
        {
            Id = team.Id,
            Name = team.Name
        };

        private static HockeyStandings.TeamRecord ToTeamRecordMessage(TeamRecord record) => new HockeyStandings.TeamRecord
        {
            Team = ToTeamMessage(record.Team),
            ConferenceRank = record.ConferenceRank,
            DivisionRank = record.DivisionRank,
            Points = record.Points,
            RegulationWins = record.RegulationWins,
            WildCardRank = record.WildCardRank
        };
    }
}
