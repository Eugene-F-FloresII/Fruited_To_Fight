using System.Collections.Generic;
using Collection;
using Data;
using Shared.Enums;
using UnityEngine;
using Controllers;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Gameplay.Enemies
{
    public class EnemyAfflictionHandler : MonoBehaviour
    {
        // Shared static pools across all enemies
        private static Dictionary<AfflictionType, ObjectPool<GameObject>> _visualPools = new();
        private static Dictionary<AfflictionType, AsyncOperationHandle<GameObject>> _prefabHandles = new();
        
        // Instance-level tracking
        private Dictionary<AfflictionType, AfflictionState> _activeAfflictions = new();
        private Dictionary<AfflictionType, GameObject> _activeVisuals = new();
        private List<AfflictionType> _expiredKeys = new();

        public static async UniTask PreWarmPoolsAsyncAddress(List<AfflictionConfig> configs, Transform poolParent)
        {
            foreach (var config in configs)
            {
                if (config == null || config.EnemyVisualPrefabReference == null || !config.EnemyVisualPrefabReference.RuntimeKeyIsValid()) continue;
                
                // Load prefab via Addressables if not already loaded
                if (!_prefabHandles.ContainsKey(config.Type))
                {
                    var handle = config.EnemyVisualPrefabReference.LoadAssetAsync<GameObject>();
                    await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        _prefabHandles[config.Type] = handle;
                        
                        GameObject prefab = handle.Result;
                        
                        _visualPools[config.Type] = new ObjectPool<GameObject>(
                            createFunc: () =>
                            {
                                GameObject obj = Instantiate(prefab, poolParent);
                                obj.SetActive(false);
                                return obj;
                            },
                            actionOnGet: obj => obj.SetActive(true),
                            actionOnRelease: obj =>
                            {
                                obj.SetActive(false);
                                if (obj != null && poolParent != null) {
                                    obj.transform.SetParent(poolParent, false);
                                }
                            },
                            actionOnDestroy: Destroy,
                            collectionCheck: false,
                            defaultCapacity: 5,
                            maxSize: 50
                        );
                        
                        // Pre-warm a few instances
                        var temp = new List<GameObject>();
                        for (int i = 0; i < 5; i++) temp.Add(_visualPools[config.Type].Get());
                        foreach (var obj in temp) _visualPools[config.Type].Release(obj);
                    }
                }
            }
        }

        public static void ReleasePoolsOP()
        {
            foreach (var pool in _visualPools.Values)
            {
                pool.Clear();
            }
            _visualPools.Clear();

            foreach (var handle in _prefabHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _prefabHandles.Clear();
        }

        private void Update()
        {
            if (_activeAfflictions.Count == 0) return;

            _expiredKeys.Clear();

            foreach (var kvp in _activeAfflictions)
            {
                kvp.Value.Tick(Time.deltaTime);

                if (kvp.Value.IsExpired)
                {
                    _expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in _expiredKeys)
            {
                if (_activeAfflictions.TryGetValue(key, out var state))
                {
                    state.Dispose();
                    _activeAfflictions.Remove(key);
                    
                    // Return visual to pool
                    if (_activeVisuals.TryGetValue(key, out var visualObj))
                    {
                        if (_visualPools.TryGetValue(key, out var pool))
                        {
                            pool.Release(visualObj);
                        }
                        else
                        {
                            Destroy(visualObj);
                        }
                        _activeVisuals.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Applies an affliction to the enemy. Refreshes if the same type is already active.
        /// </summary>
        public void ApplyAffliction(AfflictionConfig config, EnemyController controller)
        {
            if (config == null) return;

            if (_activeAfflictions.TryGetValue(config.Type, out var existingState))
            {
                existingState.Refresh(config);
                return;
            }

            AfflictionState newState = CreateAfflictionState(config.Type);
            if (newState != null)
            {
                _activeAfflictions[config.Type] = newState;
                newState.Initialize(controller, config);

                // Attach visual from pool
                if (!_activeVisuals.ContainsKey(config.Type) && _visualPools.TryGetValue(config.Type, out var pool))
                {
                    GameObject visualObj = pool.Get();
                    visualObj.transform.SetParent(transform, false);
                    visualObj.transform.localPosition = Vector3.zero;
                    _activeVisuals[config.Type] = visualObj;
                }
            }
        }

        /// <summary>
        /// Clears all active afflictions. Called when the enemy is returned to pool.
        /// </summary>
        public void ClearAllAfflictions()
        {
            foreach (var kvp in _activeAfflictions)
            {
                kvp.Value.Dispose();
            }
            _activeAfflictions.Clear();
            
            // Return all visuals to pool
            foreach (var kvp in _activeVisuals)
            {
                if (_visualPools.TryGetValue(kvp.Key, out var pool))
                {
                    pool.Release(kvp.Value);
                }
                else
                {
                    Destroy(kvp.Value);
                }
            }
            _activeVisuals.Clear();
        }

        /// <summary>
        /// Creates a new affliction state instance based on the type.
        /// </summary>
        private AfflictionState CreateAfflictionState(AfflictionType type)
        {
            return type switch
            {
                AfflictionType.Burn => new BurnState(),
                AfflictionType.Ice => new IceState(),
                AfflictionType.Weakness => new WeaknessState(),
                AfflictionType.Lightning => new LightningState(),
                _ => null
            };
        }
    }
}
