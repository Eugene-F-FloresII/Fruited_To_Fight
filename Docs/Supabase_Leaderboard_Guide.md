# Supabase Leaderboard Implementation Guide

This guide walks you through the implementation of a lightweight, highly compatible leaderboard system using the Supabase REST API in **Fruited to Fight**. 

Rather than importing a heavy, DLL-bloated C# SDK (which frequently causes issues in WebGL builds, iOS/Android AOT compilation, and managed code stripping), we implement a streamlined client using Unity's built-in `UnityWebRequest` and `UniTask`.

---

## 1. Supabase Database Schema

The implementation is designed to be fully configurable. It assumes a database table (e.g., `leaderboard` or `scores`) with the following typical structure:

| Column Name | Data Type | Notes |
|---|---|---|
| `id` | int8 or uuid | Primary Key, Auto-generated |
| `created_at` | timestamptz | Auto-generated default `now()` |
| `player_name` | text | Name of the player |
| `score` | int8 | Player's score/seeds collected |
| `rounds_survived` | int4 | Number of rounds completed |

### Required Row Level Security (RLS) Policies & Data API Privileges

By default, Supabase secures new tables and blocks external HTTP/REST client requests. You must explicitly expose the table to the Data API and configure RLS read/write access:

#### 1. Expose the Table to the Data API (API DISABLED status)
Supabase restricts table access to API roles. To enable access, go to the **SQL Editor** in your Supabase Dashboard and run:
```sql
-- Grant access privileges on the Leaderboard table to public API roles
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."Leaderboard" TO anon, authenticated;
```

#### 2. Configure RLS Policies
Ensure RLS is enabled on your `Leaderboard` table, then create the following two policies under **Database** -> **Policies**:

* **SELECT (Read access)**:
  - **Template:** "Enable read access for all users"
  - **Policy Command:** `SELECT`
  - **Target Roles:** `anon`, `authenticated`
  - **Expression (`using`):** `true`

* **INSERT (Write access)**:
  - **Template:** "Enable insert for anonymous users only"
  - **Policy Command:** `INSERT`
  - **Target Roles:** `anon`, `authenticated`
  - **Expression (`with check`):** `true`

---

## 2. Core Scripts & Architecture

We will create three C# scripts in the project:

### A. Data Model: `LeaderboardEntry.cs`
Stores the data structure of a leaderboard record. It maps the column names from your database to C# properties using `Newtonsoft.Json`.

### B. Manager: `SupabaseLeaderboardManager.cs`
A persistent manager that handles Web Requests to the Supabase REST endpoint:
- **`SubmitScoreAsync`**: Sends a `POST` request to insert a row.
- **`FetchTopScoresAsync`**: Sends a `GET` request with query parameters to retrieve the top $N$ scores (`/rest/v1/table_name?order=score.desc&limit=N`).

### C. UI Controller: `LeaderboardUI.cs`
Binds UI panels, input fields, and text lists to the database. Displays Rank, Player Name, Score, and Rounds Survived.

---

## 3. Detailed Code Implementation

Here is how the files will be structured:

### `LeaderboardEntry.cs` (Data Model)
Placed in: `Assets/Scripts/Data/LeaderboardEntry.cs`
```csharp
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
```

### `SupabaseLeaderboardManager.cs` (REST API Client)
Placed in: `Assets/Scripts/Managers/SupabaseLeaderboardManager.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json;
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
        [SerializeField] private string _tableName = "leaderboard";

        [Header("Column Mappings (Must match Supabase columns)")]
        [SerializeField] private string _colPlayerName = "player_name";
        [SerializeField] private string _colScore = "score";
        [SerializeField] private string _colRoundsSurvived = "rounds_survived";

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
                { _colScore, score },
                { _colRoundsSurvived, rounds }
            };
            
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

                        foreach (var raw in rawEntries)
                        {
                            entries.Add(new LeaderboardEntry
                            {
                                PlayerName = raw.ContainsKey(_colPlayerName) ? raw[_colPlayerName]?.ToString() : "Unknown",
                                Score = raw.ContainsKey(_colScore) ? Convert.ToInt32(raw[_colScore]) : 0,
                                RoundsSurvived = raw.ContainsKey(_colRoundsSurvived) ? Convert.ToInt32(raw[_colRoundsSurvived]) : 0
                            });
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
    }
}
```

---

## 4. UI Integration & Display

We will create a sub-panel inside `ResultSystemPanel` (in the gameplay HUD) to submit scores and display the leaderboards immediately.

### Result Screen Integration (`ResultSystemPanel.cs`)
When `ShowResults` runs (i.e. on Game Over or Level Completed):
1. Show an input field for the player's name.
2. When the user clicks the "Submit Score" button:
   - Call `SupabaseLeaderboardManager.Instance.SubmitScoreAsync(...)` using the current player stats (rounds completed and score/seeds).
   - Once submitted, fetch the top scores and populate the leaderboard list.

---

## 5. Editor Setup & Prefab Configuration Checklist

Follow these steps to fully configure the Leaderboard in the Unity Editor:

### A. Manager Configuration
1. Locate the `SupabaseManager` GameObject in the `MainMenu.unity` scene (it has the `SupabaseLeaderboardManager` script attached).
2. Verify the credentials, Table Name (`Leaderboard`), and column keys are correctly mapped.
3. Exposes **Debug Settings** (`_testPlayerName`, `_testScore`, `_testRounds`) and a clickable **"Submit Test Score"** inspector button (utilizing `NaughtyAttributes`) to test score insertion during Play Mode.

### B. Leaderboard Panel Prefab Setup
Open the isolation prefab stage for [LeaderboardPanel.prefab](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Prefabs/Ui/Leaderboard/LeaderboardPanel.prefab) and perform the following:
1. **Components:** Ensure both `LeaderboardUI` and `LeaderboardController` are attached to the root of the prefab.
2. **Placeholders:** Create simple UI placeholders under `Content_Body` for status display:
   - `Img_LoadingOverlay` (dark background image with "Loading..." text, deactivated by default).
   - `Txt_NoScores` (TextMeshProUGUI displaying "No scores found.", deactivated by default).
3. **Ghost Rows:** Make sure **no** static placeholder `Content_PlayerRank` child rows are left under `ScrollMenu/Content` (delete them from the prefab to prevent empty/duplicate rows at runtime).
4. **Inspector Binding:**
   - **`LeaderboardUI` component:**
     - Drag `ScrollMenu/Content` into `_entryContainer`.
     - Drag the `Img_LoadingOverlay` and `Txt_NoScores` GameObjects into their respective slots.
     - Leave the `_entryPrefab` slot **unassigned (null)**. It is configured as an `AssetReferenceGameObject` for Addressables.
   - **`LeaderboardController` component:**
     - Drag the root `CanvasGroup` component into `_canvasGroup`.
     - Drag `Content_Leaderboard` into `_contentLeaderboard`.
     - Drag `Content_Leaderboard/Content_Header/Btn_ExitLeaderboard` into `_exitButton`.
     - Link `_leaderboardUI` to the root `LeaderboardUI` component.
5. **Addressables Configuration:** Drag your [Content_PlayerRank.prefab](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Prefabs/Ui/Leaderboard/Content_PlayerRank.prefab) Addressable asset from your Addressables Group panel onto the empty **`Entry Prefab`** asset reference slot on the `LeaderboardUI` inspector component. Save and close the prefab stage.

### C. Main Menu Scene Setup
1. In the `MainMenu.unity` scene, select `MainMenuCanvas/Container/Btn_LeaderBoard`.
2. Configure the button's **`onClick`** persistent event list:
   - Target: `MainMenuCanvas/LeaderboardPanel` GameObject.
   - Function: `GameObject.SetActive` (checked / set to `true`).
3. When clicked, this will set the panel active, triggering the `LeaderboardController`'s `OnEnable()` script, which plays the fade-in/pop-up animation and auto-refreshes the scores.
