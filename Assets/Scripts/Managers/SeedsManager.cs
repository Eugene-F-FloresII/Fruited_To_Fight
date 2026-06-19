using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Seed;
using Shared.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace Managers
{
    public class SeedsManager : MonoBehaviour
    {
        [Header("Seed Config")]
        [SerializeField] private AssetReferenceT<SeedConfig> _seedConfigReference;

        [Header("Object Pooling Settings")]
        [SerializeField] private Transform _pooledTransform;
        [SerializeField] private int _defaultSeedsToPool = 100;
        [SerializeField] private int _maxSeedsInPool = 100;
        [SerializeField] private SeedSeeker _seedSeeker;
        
        private SeedConfig _seedConfig;
        private Seed _pooledSeed;
        private ObjectPool<Seed> _seedPool;
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

                InitializePool();
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

        private void InitializePool()
        {
            _seedPool = new ObjectPool<Seed>(
                createFunc: CreateSeedInstance,
                actionOnGet: OnTakeSeedFromPool,
                actionOnRelease: OnReturnedSeedToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: false,
                defaultCapacity: _defaultSeedsToPool,
                maxSize: _maxSeedsInPool
            );

            // Pre-warm the pool to create initial instances
            List<Seed> prewarmedSeeds = new List<Seed>(_defaultSeedsToPool);
            for (int i = 0; i < _defaultSeedsToPool; i++)
            {
                prewarmedSeeds.Add(_seedPool.Get());
            }
            foreach (var seed in prewarmedSeeds)
            {
                _seedPool.Release(seed);
            }
        }

        private Seed CreateSeedInstance()
        {
            Seed pooledInstance = Instantiate(_pooledSeed, _pooledTransform);
            pooledInstance.gameObject.SetActive(false);
            pooledInstance.Initialize(_seedSeeker, ReleaseSeed);
            return pooledInstance;
        }

        private void ReleaseSeed(Seed seed)
        {
            if (_seedPool != null)
            {
                _seedPool.Release(seed);
            }
        }

        private void OnTakeSeedFromPool(Seed seed)
        {
            // Handled explicitly in HandleEnemyDeath to prevent visual/position glitch
        }

        private void OnReturnedSeedToPool(Seed seed)
        {
            if (seed != null)
            {
                seed.gameObject.SetActive(false);
            }
        }

        private void OnDestroyPoolObject(Seed seed)
        {
            if (seed != null)
            {
                Destroy(seed.gameObject);
            }
        }

        private Seed GetPooledSeed()
        {
            if (_seedPool == null)
            {
                return null;
            }
            return _seedPool.Get();
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
