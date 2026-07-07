using Newtonsoft.Json;

namespace Data
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        [JsonProperty("player_name")]
        public string PlayerName;

        [JsonProperty("score")]
        public int Score;

        [JsonProperty("rounds_survived")]
        public int RoundsSurvived;

        [JsonProperty("created_at")]
        public string CreatedAt;
    }
}
