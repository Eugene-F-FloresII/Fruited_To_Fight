using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Gameplay.UI;
using Shared.Events;
using Cysharp.Threading.Tasks;

namespace Controllers
{
    public class BossUIController : MonoBehaviour
    {
        [Header("Boss List")]
        [SerializeField] private List<AssetReferenceT<EnemyBossConfig>> _bossList;
        [SerializeField] private string _bossHealthLabel = "BossHealth";
        [SerializeField] private Transform _listTransform;
        [SerializeField] private CanvasGroup _canvasGroup;

        private List<BossHealth> _activeUIs = new();

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            Events_Boss.OnBossSpawned += HandleBossSpawned;
        }

        private void OnDisable()
        {
            Events_Boss.OnBossSpawned -= HandleBossSpawned;
        }

        private async void HandleBossSpawned(EnemyController boss, EnemyBossConfig config)
        {
            if (string.IsNullOrEmpty(_bossHealthLabel))
            {
                Debug.LogWarning("BossHealth Addressables label is not set on BossUIController.", this);
                return;
            }

            Transform parentTransform = _listTransform != null ? _listTransform : transform;

            // Instantiate prefab asynchronously using Addressables label
            GameObject bossUIObj = await Addressables.InstantiateAsync(_bossHealthLabel, parentTransform).ToUniTask();
            if (bossUIObj == null) return;

            BossHealth bossUI = bossUIObj.GetComponent<BossHealth>();
            if (bossUI == null)
            {
                Debug.LogError("Instantiated prefab is missing BossHealth component.", this);
                Addressables.ReleaseInstance(bossUIObj);
                return;
            }

            _activeUIs.Add(bossUI);

            // Turn on the panel CanvasGroup when any boss UI is spawned
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            bossUI.Initialize(boss, config, HandleUIFinished);
        }

        private void HandleUIFinished(BossHealth bossUI)
        {
            _activeUIs.Remove(bossUI);

            // Release the Addressables instance (this will also destroy the GameObject)
            if (bossUI != null)
            {
                Addressables.ReleaseInstance(bossUI.gameObject);
            }

            // If all boss UI elements are defeated and destroyed, set alpha to 0
            if (_activeUIs.Count == 0)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
        }
    }
}
