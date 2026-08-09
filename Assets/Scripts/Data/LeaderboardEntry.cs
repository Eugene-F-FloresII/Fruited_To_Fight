using Newtonsoft.Json;

namespace Data
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("player_id")]
        public string PlayerId;

        [JsonProperty("player_name")]
        public string PlayerName;

        [JsonProperty("score")]
        public int Score;

        [JsonProperty("rounds_survived")]
        public int RoundsSurvived;

        [JsonProperty("map_id")]
        public int MapId;

        [JsonProperty("created_at")]
        public string CreatedAt;
    }
}
