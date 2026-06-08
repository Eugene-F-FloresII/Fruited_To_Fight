using System;
using System.Collections.Generic;
using System.Threading;
using Data;
using Shared.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Gameplay.Enemies
{
    public class EnemyAffliction : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _afflictionGameObjects;

        [Header("Don't add anything here")] 
        [SerializeField] private AfflictionConfig _afflictionConfig;

        private CancellationTokenSource _durationCancellation;

        public void ToggleVisual(AfflictionType afflictionType, bool isActive)
        {
            var affliction = GetAffliction(afflictionType);
            if (affliction != null)
            {
                affliction.SetActive(isActive);
            }
            
            if (isActive)
            {
                AddAffliction(GetAfflictionConfigKey(afflictionType)).Forget();
            }
            else
            {
                CancelDuration();
                _afflictionConfig = null;
            }
        }

        private GameObject GetAffliction(AfflictionType afflictionType)
        {
            return afflictionType switch
            {
                AfflictionType.Burn => _afflictionGameObjects[0],
                AfflictionType.Ice => _afflictionGameObjects[1],
                AfflictionType.Weakness => _afflictionGameObjects[2],
                AfflictionType.Lightning => _afflictionGameObjects[3],
                _ => null
            };
        }

        private string GetAfflictionConfigKey(AfflictionType afflictionType)
        {
            return afflictionType switch
            {
                AfflictionType.Burn => "BurnAffliction",
                AfflictionType.Ice => "IceAffliction",
                AfflictionType.Weakness => "WeaknessAffliction",
                AfflictionType.Lightning => "LightningAffliction",
                _ => null
            };
        }
        
        public async UniTask AddAffliction(string afflictionKey)
        {
            var handle = Addressables.LoadAssetAsync<AfflictionConfig>(afflictionKey);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _afflictionConfig = handle.Result;
                StartDurationTimer(_afflictionConfig.Duration).Forget();
            }
            else
            {
                Debug.LogError($"Failed to load UpgradeData with key '{afflictionKey}'");
            }
        }
        
        private async UniTaskVoid StartDurationTimer(float duration)
        {
            CancelDuration();
            _durationCancellation = new CancellationTokenSource();
            var token = _durationCancellation.Token;

            try
            {
                await UniTask.WaitForSeconds(duration, cancellationToken: token);
                
                _afflictionConfig = null;
                foreach (var go in _afflictionGameObjects)
                {
                    if (go != null && go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when a new affliction replaces the current one
            }
        }

        private void CancelDuration()
        {
            if (_durationCancellation != null)
            {
                _durationCancellation.Cancel();
                _durationCancellation.Dispose();
                _durationCancellation = null;
            }
        }

        private void OnDestroy()
        {
            CancelDuration();
        }
    }
}
