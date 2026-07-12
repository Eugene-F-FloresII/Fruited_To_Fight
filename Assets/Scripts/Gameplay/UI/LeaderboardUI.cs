using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gameplay.UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform _entryContainer;
        [SerializeField] private AssetReferenceGameObject _entryPrefab;
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private TextMeshProUGUI _noScoresText;

        [Header("Settings")]
        [SerializeField] private int _limit = 10;

        private List<GameObject> _instantiatedRows = new List<GameObject>();

        public void ClearLeaderboard()
        {
            foreach (var row in _instantiatedRows)
            {
                if (row != null)
                {
                    Addressables.ReleaseInstance(row);
                }
            }
            _instantiatedRows.Clear();
        }

        public async UniTask RefreshLeaderboardAsync()
        {
            ClearLeaderboard();

            if (_loadingOverlay != null) _loadingOverlay.SetActive(true);
            if (_noScoresText != null) _noScoresText.gameObject.SetActive(false);

            if (Shared.Events.Events_Leaderboard.OnFetchLeaderboard == null)
            {
                Debug.LogError("Events_Leaderboard.OnFetchLeaderboard is not registered.");
                if (_loadingOverlay != null) _loadingOverlay.SetActive(false);
                return;
            }

            var entries = await Shared.Events.Events_Leaderboard.OnFetchLeaderboard.Invoke(_limit);

            if (_loadingOverlay != null) _loadingOverlay.SetActive(false);

            if (entries == null || entries.Count == 0)
            {
                if (_noScoresText != null) _noScoresText.gameObject.SetActive(true);
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (_entryPrefab == null || !_entryPrefab.RuntimeKeyIsValid()) continue;
                var rowGo = await _entryPrefab.InstantiateAsync(_entryContainer).ToUniTask();
                _instantiatedRows.Add(rowGo);

                // Populate row data
                PopulateRow(rowGo, i + 1, entry);
            }
        }

        private void PopulateRow(GameObject rowGo, int rank, LeaderboardEntry entry)
        {
            var texts = rowGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            // Map text values based on the GameObjects' names (case-insensitive search)
            foreach (var t in texts)
            {
                string name = t.gameObject.name.ToLower();
                if (name.Contains("rank"))
                {
                    t.text = rank.ToString();
                }
                else if (name.Contains("name") || name.Contains("player"))
                {
                    t.text = entry.PlayerName;
                }
                else if (name.Contains("score") || name.Contains("seed"))
                {
                    t.text = entry.Score.ToString("N0");
                }
                else if (name.Contains("round") || name.Contains("survive"))
                {
                    t.text = entry.RoundsSurvived.ToString();
                }
            }
        }
    }
}
