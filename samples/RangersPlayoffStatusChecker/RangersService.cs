using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RangersPlayoffStatusChecker
{
    public class RangersService
    {
        private const string StandingsUrl = "https://statsapi.web.nhl.com/api/v1/standings";
        private const int RangersId = 3;

        private readonly HttpClient _client = new HttpClient();
        private readonly JsonSerializerOptions _jsonOptions;

        public RangersService()
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _jsonOptions.Converters.Add(new StringIntConverter());
        }

        public async Task<TeamRecord> GetRangersRecord()
        {
            var json = await _client.GetStringAsync(StandingsUrl).ConfigureAwait(false);
            var standings = JsonSerializer.Deserialize<Standings>(json, _jsonOptions);

            return standings.Records
                .SelectMany(record => record.TeamRecords)
                .Single(record => record.Team.Id == RangersId);
        }

        private class StringIntConverter : JsonConverter<int>
        {
            public override int Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                    return int.Parse(reader.GetString());

                return reader.GetInt32();
            }

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            {
            }
        }
    }

    public static class RecordExtensions
    {
        public static bool IsPlayoffBound(this TeamRecord record) =>
            record.DivisionRank <= 3 || record.WildCardRank <= 2;
    }

    public class Standings
    {
        public IList<Record> Records { get; set; }
    }

    public class Record
    {
        public string StandingsType { get; set; }
        public IList<TeamRecord> TeamRecords { get; set; }
    }

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TeamRecord
    {
        public Team Team { get; set; }

        public int RegulationWins { get; set; }
        public int Points { get; set; }

        public int DivisionRank { get; set; }
        public int ConferenceRank { get; set; }
        public int WildCardRank { get; set; }
    }
}
