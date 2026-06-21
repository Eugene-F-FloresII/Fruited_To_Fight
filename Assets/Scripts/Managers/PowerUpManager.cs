using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Powerups;
using Shared.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace Managers
{
    public class PowerUpManager : MonoBehaviour
    {
        [Header("Power Up Config")]
        [SerializeField] private AssetReferenceT<PowerUpManagerConfig> _managerConfigReference;

        [Header("Object Pooling Settings")]
        [SerializeField] private Transform _pooledTransform;
        [SerializeField] private int _defaultPowerUpsToPool = 10;
        [SerializeField] private int _maxPowerUpsInPool = 50;

        private class LoadedPowerUpData
        {
            public PowerUpConfig Config;
            public float SpawnChance;
            public ObjectPool<PowerUp> Pool;
        }

        private PowerUpManagerConfig _managerConfig;
        private List<LoadedPowerUpData> _loadedPowerUps = new List<LoadedPowerUpData>();
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
            InitializePowerUpManager().Forget();
        }

        private void OnDestroy()
        {
            // Release the config reference
            if (_managerConfigReference.IsValid())
            {
                _managerConfigReference.ReleaseAsset();
            }

            // Release each loaded PowerUpConfig addressable
            if (_managerConfig != null && _managerConfig.PowerUpSpawnList != null)
            {
                foreach (var spawnData in _managerConfig.PowerUpSpawnList)
                {
                    if (spawnData.PowerUpConfigReference != null && spawnData.PowerUpConfigReference.IsValid())
                    {
                        spawnData.PowerUpConfigReference.ReleaseAsset();
                    }
                }
            }
        }

        private async UniTask InitializePowerUpManager()
        {
            if (_isInitialized || _isInitializing)
            {
                return;
            }

            _isInitializing = true;

            try
            {
                _managerConfig = await _managerConfigReference.LoadAssetAsync<PowerUpManagerConfig>().ToUniTask();

                if (_managerConfig == null)
                {
                    Debug.LogWarning($"{nameof(PowerUpManager)} could not load PowerUpManagerConfig.", this);
                    return;
                }

                if (_pooledTransform == null)
                {
                    _pooledTransform = transform;
                }

                // Load all configured power-ups and build their pools
                foreach (var spawnData in _managerConfig.PowerUpSpawnList)
                {
                    if (spawnData.PowerUpConfigReference == null || !spawnData.PowerUpConfigReference.RuntimeKeyIsValid())
                    {
                        continue;
                    }

                    try
                    {
                        PowerUpConfig powerUpConfig = await spawnData.PowerUpConfigReference.LoadAssetAsync<PowerUpConfig>().ToUniTask();
                        if (powerUpConfig == null || powerUpConfig.PowerUp == null)
                        {
                            Debug.LogWarning($"Failed to load PowerUpConfig or prefab is null for reference: {spawnData.PowerUpConfigReference}", this);
                            continue;
                        }

                        ObjectPool<PowerUp> pool = CreatePoolForPowerUp(powerUpConfig);

                        _loadedPowerUps.Add(new LoadedPowerUpData
                        {
                            Config = powerUpConfig,
                            SpawnChance = spawnData.SpawnChance,
                            Pool = pool
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, this);
                    }
                }

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

        private ObjectPool<PowerUp> CreatePoolForPowerUp(PowerUpConfig config)
        {
            ObjectPool<PowerUp> pool = new ObjectPool<PowerUp>(
                createFunc: () => CreatePowerUpInstance(config),
                actionOnGet: OnTakePowerUpFromPool,
                actionOnRelease: OnReturnedPowerUpToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: false,
                defaultCapacity: _defaultPowerUpsToPool,
                maxSize: _maxPowerUpsInPool
            );

            // Pre-warm pool
            List<PowerUp> prewarmed = new List<PowerUp>(_defaultPowerUpsToPool);
            for (int i = 0; i < _defaultPowerUpsToPool; i++)
            {
                prewarmed.Add(pool.Get());
            }
            foreach (var item in prewarmed)
            {
                pool.Release(item);
            }

            return pool;
        }

        private PowerUp CreatePowerUpInstance(PowerUpConfig config)
        {
            PowerUp instance = Instantiate(config.PowerUp, _pooledTransform);
            instance.gameObject.SetActive(false);

            instance.InitializePoolCallback((powerUp) =>
            {
                // Find matching pool to return instance to
                var data = _loadedPowerUps.Find(d => d.Config == config);
                if (data != null && data.Pool != null)
                {
                    data.Pool.Release(powerUp);
                }
            });

            return instance;
        }

        private void OnTakePowerUpFromPool(PowerUp powerUp)
        {
            // Handled explicitly in HandleEnemyDeath to prevent visual/position glitch
        }

        private void OnReturnedPowerUpToPool(PowerUp powerUp)
        {
            if (powerUp != null)
            {
                powerUp.gameObject.SetActive(false);
            }
        }

        private void OnDestroyPoolObject(PowerUp powerUp)
        {
            if (powerUp != null)
            {
                Destroy(powerUp.gameObject);
            }
        }

        private void HandleEnemyDeath(Transform enemyTransform)
        {
            if (!_isInitialized || enemyTransform == null || _loadedPowerUps.Count == 0)
            {
                return;
            }

            // Roll for global drop chance
            float globalRoll = UnityEngine.Random.value;
            if (globalRoll > _managerConfig.GlobalDropChance)
            {
                return;
            }

            // Calculate total weight of loaded power-ups
            float totalWeight = 0f;
            foreach (var data in _loadedPowerUps)
            {
                totalWeight += data.SpawnChance;
            }

            if (totalWeight <= 0f)
            {
                return;
            }

            // Perform weighted selection
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            LoadedPowerUpData selectedData = null;

            foreach (var data in _loadedPowerUps)
            {
                cumulative += data.SpawnChance;
                if (roll <= cumulative)
                {
                    selectedData = data;
                    break;
                }
            }

            if (selectedData == null || selectedData.Pool == null)
            {
                return;
            }

            PowerUp powerUp = selectedData.Pool.Get();
            if (powerUp == null)
            {
                return;
            }

            Transform powerUpTransform = powerUp.transform;
            powerUpTransform.position = enemyTransform.position;
            powerUpTransform.rotation = Quaternion.identity;
            powerUp.gameObject.SetActive(true);
        }
    }
}
