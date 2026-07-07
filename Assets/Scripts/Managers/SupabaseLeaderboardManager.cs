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
        public static SupabaseLeaderboardManager Instance { get; private set; }

        [Header("Supabase Settings")]
        [SerializeField] private string _supabaseUrl = "https://fjanaixxiwwktqhazrbp.supabase.co";
        [SerializeField] private string _anonKey = "sb_publishable_H6Yj0UEXiLe8yjJwzFrk1g_fV9E1FPl";
        [SerializeField] private string _tableName = "Leaderboard";

        [Header("Column Mappings (Must match Supabase columns)")]
        [SerializeField] private string _colPlayerName = "player_name";
        [SerializeField] private string _colScore = "score";
        [SerializeField] private string _colRoundsSurvived = "rounds_survived";

        [Header("Debug Settings")]
        [SerializeField] private string _testPlayerName = "TestPlayer";
        [SerializeField] private int _testScore = 1000;
        [SerializeField] private int _testRounds = 5;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Submits a player's score to the Supabase database.
        /// </summary>
        public async UniTask<bool> SubmitScoreAsync(string playerName, int score, int rounds)
        {
            string url = $"{_supabaseUrl}/rest/v1/{_tableName}";
            
            // Build json payload dynamically to match column maps
            var payload = new Dictionary<string, object>
            {
                { _colPlayerName, playerName },
                { _colScore, score }
            };
            
            // Only submit rounds if the column is configured
            if (!string.IsNullOrEmpty(_colRoundsSurvived))
            {
                payload.Add(_colRoundsSurvived, rounds);
            }
            
            string json = JsonConvert.SerializeObject(payload);
            byte[] bodyData = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
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
        public async UniTask<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10)
        {
            // Endpoint query: order by score descending, limit response
            string url = $"{_supabaseUrl}/rest/v1/{_tableName}?select=*&order={_colScore}.desc&limit={limit}";

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
