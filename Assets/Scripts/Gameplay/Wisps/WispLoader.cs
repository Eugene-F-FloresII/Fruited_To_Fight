using System.Collections.Generic;
using Data;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Gameplay.Wisps
{
    public class WispLoader : MonoBehaviour
    {
        [SerializeField] private Transform _leftSpawner;
        [SerializeField] private Transform _centerSpawner;
        [SerializeField] private Transform _rightSpawner;

        private bool _leftOccupied;
        private bool _centerOccupied;
        private bool _rightOccupied;
        
        private WispConfig _wispConfig;
        private List<GameObject> _spawnedWisps = new List<GameObject>();

        private void OnEnable()
        {
            Events_Wisps.OnChosenWisp += InitializeWisp;
            Events_Game.OnGameRestarted += DespawnAllWisps;
            Events_Game.OnGameExited += DespawnAllWisps;
            Events_Character.OnPlayerDeath += DespawnAllWisps;
        }

        private void OnDisable()
        {
            Events_Wisps.OnChosenWisp -= InitializeWisp;
            Events_Game.OnGameRestarted -= DespawnAllWisps;
            Events_Game.OnGameExited -= DespawnAllWisps;
            Events_Character.OnPlayerDeath -= DespawnAllWisps;
        }

        private void DespawnAllWisps()
        {
            foreach (var wisp in _spawnedWisps)
            {
                if (wisp != null)
                {
                    Destroy(wisp);
                }
            }
            _spawnedWisps.Clear();
            _leftOccupied = false;
            _centerOccupied = false;
            _rightOccupied = false;
        }

        private void InitializeWisp(string wisp)
        {
            PreparingWisp(wisp).Forget();
        }

        private async UniTask PreparingWisp(string wisp)
        {
            var handle = Addressables.LoadAssetAsync<WispConfig>(wisp);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _wispConfig = handle.Result;
                PreparedWisp(_wispConfig).Forget();
            }
            else
            {
                Debug.LogError($"Failed to load WispConfig with key '{wisp}'");
            }
        }

        private async UniTask PreparedWisp(WispConfig wispConfig)
        {
            GameObject wispPrefab = wispConfig.WispPrefab;

            if (wispPrefab == null)
            {
                Debug.LogError($"WispPrefab is null on {wispConfig.name}");
                return;
            }

            Transform targetSpawner = null;

            if (!_leftOccupied)
            {
                targetSpawner = _leftSpawner;
                _leftOccupied = true;
            }
            else if (!_centerOccupied)
            {
                targetSpawner = _centerSpawner;
                _centerOccupied = true;
            }
            else if (!_rightOccupied)
            {
                targetSpawner = _rightSpawner;
                _rightOccupied = true;
            }

            if (targetSpawner != null)
            {
                GameObject wispInstance = Instantiate(wispPrefab, targetSpawner.position, targetSpawner.rotation, targetSpawner);
                _spawnedWisps.Add(wispInstance);
                Events_Wisps.OnSpawnedWisp?.Invoke(wispInstance);
            }
            else
            {
                Debug.LogWarning("All wisp spawn points occupied, cannot spawn wisp.");
            }
        }
    }
}
