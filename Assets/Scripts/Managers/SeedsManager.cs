using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Seed;
using Shared.Events;
using UnityEngine.AddressableAssets;

namespace Managers
{
    public class SeedsManager : MonoBehaviour
    {
        [Header("Seed Config")]
        [SerializeField] private AssetReferenceT<SeedConfig> _seedConfigReference;

        [Header("Object Pooling Settings")]
        [SerializeField] private Transform _pooledTransform;
        [SerializeField] private int _seedsAmountToPool = 20;
        [SerializeField] private SeedSeeker _seedSeeker;
        
        private SeedConfig _seedConfig;
        private Seed _pooledSeed;
        private Queue<Seed> _pooledSeeds = new();
        private bool _isInitialized;
        private bool _isInitializing;

        private void OnEnable()
        {
            Events_Seed.OnEnemyDeath += HandleEnemyDeath;
        }

        private void OnDisable()
        {
            Events_Seed.OnEnemyDeath -= HandleEnemyDeath;
        }
        
        private void Start()
        {
            InitializeSeedManager().Forget();
        }

        private void OnDestroy()
        {
            if (_seedConfigReference.IsValid())
            {
                _seedConfigReference.ReleaseAsset();
            }
        }

        private async UniTask InitializeSeedManager()
        {
            if (_isInitialized || _isInitializing)
            {
                return;
            }

            _isInitializing = true;

            try
            {
                _seedConfig = await _seedConfigReference.LoadAssetAsync<SeedConfig>().ToUniTask();

                if (_seedConfig == null || _seedConfig.SeedPrefab == null)
                {
                    Debug.LogWarning($"{nameof(SeedsManager)} could not initialize. SeedConfig or SeedPrefab is null.", this);
                    return;
                }

                _pooledSeed = _seedConfig.SeedPrefab;

                if (_pooledTransform == null)
                {
                    _pooledTransform = transform;
                }

                if (_seedSeeker == null)
                {
                    Debug.LogWarning($"{nameof(SeedsManager)} has no {nameof(SeedSeeker)} assigned. Spawned seeds will not follow the player.", this);
                }

                PoolSeeds();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void PoolSeeds()
        {
            _pooledSeeds = new Queue<Seed>();

            for (int i = 0; i < _seedsAmountToPool; i++)
            {
                Seed pooledInstance = Instantiate(_pooledSeed, _pooledTransform);
                pooledInstance.Initialize(_seedSeeker);
                pooledInstance.gameObject.SetActive(false);
                _pooledSeeds.Enqueue(pooledInstance);
            }
        }

        private Seed GetPooledSeed()
        {
            if (_pooledSeeds == null || _pooledSeeds.Count == 0)
            {
                Debug.LogWarning("No seeds left in pool", this);
                return null;
            }

            int poolSize = _pooledSeeds.Count;

            for (int i = 0; i < poolSize; i++)
            {
                Seed seed = _pooledSeeds.Dequeue();
                _pooledSeeds.Enqueue(seed);

                if (seed != null && !seed.gameObject.activeSelf)
                {
                    return seed;
                }
            }

            Debug.LogWarning("All pooled seeds are currently active", this);
            return null;
        }

        private void HandleEnemyDeath(Transform enemyTransform)
        {
            if (!_isInitialized || enemyTransform == null)
            {
                return;
            }

            Seed seed = GetPooledSeed();

            if (seed == null)
            {
                return;
            }

            Transform seedTransform = seed.transform;
            seedTransform.position = enemyTransform.position;
            seedTransform.rotation = Quaternion.identity;
            seed.gameObject.SetActive(true);
        }
    }

}
