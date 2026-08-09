using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
    public class SupabaseLeaderboardManager : MonoBehaviour
    {
        private static bool _exists;

        [Header("Supabase Settings")]
        [SerializeField] private string _supabaseUrl = "https://fjanaixxiwwktqhazrbp.supabase.co";
        [SerializeField] private string _anonKey = "sb_publishable_H6Yj0UEXiLe8yjJwzFrk1g_fV9E1FPl";
        [SerializeField] private string _tableName = "Leaderboard";

        [Header("Column Mappings (Must match Supabase columns)")]
        [SerializeField] private string _colPlayerName = "player_name";
        [SerializeField] private string _colScore = "score";
        [SerializeField] private string _colRoundsSurvived = "rounds_survived";
        [SerializeField] private string _colMapId = "map_id";
        [SerializeField] private string _colPlayerId = "player_id";

        [Header("Map Settings")]
        [SerializeField] private Shared.Enums.MapType _currentMap;

        [Header("Debug Settings")]
        [SerializeField] private string _testPlayerName = "TestPlayer";
        [SerializeField] private int _testScore = 1000;
        [SerializeField] private int _testRounds = 5;

        private void Awake()
        {
            if (!_exists)
            {
                _exists = true;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            Shared.Events.Events_Leaderboard.OnSubmitScore = SubmitScoreAsync;
            Shared.Events.Events_Leaderboard.OnFetchLeaderboard = FetchTopScoresAsync;
        }

        private void OnDisable()
        {
            if (Shared.Events.Events_Leaderboard.OnSubmitScore == SubmitScoreAsync)
            {
                Shared.Events.Events_Leaderboard.OnSubmitScore = null;
            }
            if (Shared.Events.Events_Leaderboard.OnFetchLeaderboard == FetchTopScoresAsync)
            {
                Shared.Events.Events_Leaderboard.OnFetchLeaderboard = null;
            }
        }

        private string GetPlayerId()
        {
            if (!UnityEngine.PlayerPrefs.HasKey("PlayerID"))
            {
                UnityEngine.PlayerPrefs.SetString("PlayerID", System.Guid.NewGuid().ToString());
                UnityEngine.PlayerPrefs.Save();
            }
            return UnityEngine.PlayerPrefs.GetString("PlayerID");
        }

        /// <summary>
        /// Submits a player's score to the Supabase database.
        /// Only updates if the new score is higher than the existing score.
        /// </summary>
        public async UniTask<bool> SubmitScoreAsync(string playerName, int score, int rounds)
        {
            string playerId = GetPlayerId();
            string filterQuery = _currentMap == Shared.Enums.MapType.Grasslands 
                ? $"or=({_colMapId}.eq.0,{_colMapId}.is.null)" 
                : $"{_colMapId}=eq.{(int)_currentMap}";
                
            string checkUrl = $"{_supabaseUrl}/rest/v1/{_tableName}?select=id,{_colScore}&{_colPlayerId}=eq.{playerId}&{filterQuery}&limit=1";
            
            string existingRowId = null;
            int existingScore = -1;

            using (UnityWebRequest checkReq = UnityWebRequest.Get(checkUrl))
            {
                SetCommonHeaders(checkReq);
                try
                {
                    await checkReq.SendWebRequest().ToUniTask();
                    if (checkReq.result == UnityWebRequest.Result.Success)
                    {
                        var rawEntries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(checkReq.downloadHandler.text);
                        if (rawEntries != null && rawEntries.Count > 0)
                        {
                            var row = rawEntries[0];
                            if (row.ContainsKey("id") && row["id"] != null)
                                existingRowId = row["id"].ToString();
                            if (row.ContainsKey(_colScore) && row[_colScore] != null)
                                existingScore = Convert.ToInt32(row[_colScore]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            if (existingRowId != null && existingScore >= score)
            {
                Debug.Log($"Supabase: Existing score ({existingScore}) is higher or equal to new score ({score}). Skipping update.");
                return true; // Return true because it's not a failure, just skipped.
            }

            var payload = new Dictionary<string, object>
            {
                { _colPlayerId, playerId },
                { _colPlayerName, playerName },
                { _colScore, score },
                { _colMapId, (int)_currentMap }
            };
            
            if (!string.IsNullOrEmpty(_colRoundsSurvived))
            {
                payload.Add(_colRoundsSurvived, rounds);
            }
            
            string json = JsonConvert.SerializeObject(payload);
            byte[] bodyData = Encoding.UTF8.GetBytes(json);

            string submitUrl = $"{_supabaseUrl}/rest/v1/{_tableName}";
            string method = "POST";
            if (existingRowId != null)
            {
                submitUrl = $"{_supabaseUrl}/rest/v1/{_tableName}?id=eq.{existingRowId}";
                method = "PATCH";
            }

            using (UnityWebRequest request = new UnityWebRequest(submitUrl, method))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyData);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                SetCommonHeaders(request);
                request.SetRequestHeader("Prefer", "return=minimal");

                try
                {
                    await request.SendWebRequest().ToUniTask();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Supabase: Score submitted successfully!");
                        return true;
                    }
                    
                    Debug.LogError($"Supabase Submit Error: {request.error}\nResponse: {request.downloadHandler.text}");
                    return false;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return false;
                }
            }
        }

        /// <summary>
        /// Fetches the top scores from Supabase.
        /// </summary>
        public async UniTask<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 100)
        {
            // Endpoint query: filter by map_id, order by score descending, limit response
            string filterQuery = _currentMap == Shared.Enums.MapType.Grasslands 
                ? $"or=({_colMapId}.eq.0,{_colMapId}.is.null)" 
                : $"{_colMapId}=eq.{(int)_currentMap}";

            string url = $"{_supabaseUrl}/rest/v1/{_tableName}?select=*&{filterQuery}&order={_colScore}.desc&limit={limit}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                SetCommonHeaders(request);

                try
                {
                    await request.SendWebRequest().ToUniTask();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string jsonResult = request.downloadHandler.text;
                        
                        // Map JSON response to generic models
                        var rawEntries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonResult);
                        var entries = new List<LeaderboardEntry>();

                        if (rawEntries != null)
                        {
                            foreach (var raw in rawEntries)
                            {
                                var entry = new LeaderboardEntry
                                {
                                    PlayerName = raw.ContainsKey(_colPlayerName) && raw[_colPlayerName] != null ? raw[_colPlayerName].ToString() : "Unknown",
                                    Score = raw.ContainsKey(_colScore) && raw[_colScore] != null ? Convert.ToInt32(raw[_colScore]) : 0
                                };

                                if (!string.IsNullOrEmpty(_colRoundsSurvived) && raw.ContainsKey(_colRoundsSurvived) && raw[_colRoundsSurvived] != null)
                                {
                                    entry.RoundsSurvived = Convert.ToInt32(raw[_colRoundsSurvived]);
                                }

                                entries.Add(entry);
                            }
                        }

                        return entries;
                    }
                    
                    Debug.LogError($"Supabase Fetch Error: {request.error}\nResponse: {request.downloadHandler.text}");
                    return null;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }
        }

        private void SetCommonHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("apikey", _anonKey);
            request.SetRequestHeader("Authorization", $"Bearer {_anonKey}");
            request.SetRequestHeader("Content-Type", "application/json");
        }

        [Button("Submit Test Score")]
        private void DebugSubmitTestScore()
        {
            SubmitScoreAsync(_testPlayerName, _testScore, _testRounds).Forget();
        }
    }
}
