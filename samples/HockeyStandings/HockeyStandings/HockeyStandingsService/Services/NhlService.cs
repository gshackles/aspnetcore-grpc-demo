using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HockeyStandingsService.Services
{
    public class NhlService
    {
        private const string StandingsUrl = "https://statsapi.web.nhl.com/api/v1/standings";

        private readonly HttpClient _client = new HttpClient();
        private readonly JsonSerializerOptions _jsonOptions;

        public NhlService()
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _jsonOptions.Converters.Add(new StringIntConverter());
        }

        public async Task<IList<TeamRecord>> GetTeamRecords()
        {
            var json = await _client.GetStringAsync(StandingsUrl).ConfigureAwait(false);
            var standings = JsonSerializer.Deserialize<Standings>(json, _jsonOptions);

            return standings.Records
                .SelectMany(record => record.TeamRecords)
                .ToList();
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