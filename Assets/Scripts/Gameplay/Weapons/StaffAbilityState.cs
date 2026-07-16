using System;
using System.Threading;
using Collection;
using Controllers;
using Cysharp.Threading.Tasks;
using Data;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Shared.Enums;
using Shared.Events;
using Random = UnityEngine.Random;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Handles the Staff special ability: a meteor shower spawning meteors randomly in a radius around the player.
    /// </summary>
    public class StaffAbilityState : WeaponAbilityState
    {
        [Header("Meteor Settings")]
        [SerializeField] private AssetReferenceGameObject _meteorPrefabReference;
        [SerializeField] private Transform _target;

        private float MeteorRadius => _weaponConfig != null ? _weaponConfig.AbilityRadius : 4.0f;
        private float SpawnInterval => _weaponConfig != null ? (1.0f / Mathf.Max(0.01f, _weaponConfig.AbilitySpeed)) : 0.5f;
        private int MeteorsPerSpawn => _weaponConfig != null ? _weaponConfig.AbilitySpawnCount : 1;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<StaffAbilityState>();
            
            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _abilityCts = null;

            _cooldownCts?.Cancel();
            _cooldownCts?.Dispose();
            _cooldownCts = null;
            
            if (_meteorPrefabReference != null && _meteorPrefabReference.IsValid())
            {
                _meteorPrefabReference.ReleaseAsset();
            }
        }

        /// <summary>
        /// Triggers the execution of the staff's special ability.
        /// </summary>
        [Button("Test Weapon Ability")]
        public override void UseWeaponAbility()
        {
            if (_weaponConfig == null)
            {
                Debug.LogError("WeaponConfig is NULL on StaffAbilityState!", this);
                return;
            }

            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _abilityCts = new CancellationTokenSource();
            
            UseWeaponAbilityAsync(_abilityCts.Token).Forget();
        }

        /// <summary>
        /// Asynchronously executes the meteor shower logic.
        /// </summary>
        /// <param name="token">Cancellation token for the current execution.</param>
        public override async UniTask UseWeaponAbilityAsync(CancellationToken token)
        {
            try
            {
                if (_meteorPrefabReference == null)
                {
                    Debug.LogError("[StaffAbility] Meteor prefab reference is null!");
                    return;
                }

                GameObject meteorPrefab = await LoadMeteorPrefabAsyncAddress(token);
                if (meteorPrefab == null)
                {
                    Debug.LogError("[StaffAbility] Failed to load meteor prefab!");
                    return;
                }

                await SpawnMeteorsLoopAsync(meteorPrefab, token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[StaffAbility] Ability cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (this != null)
                {
                    _cooldownCts?.Cancel();
                    _cooldownCts?.Dispose();
                    _cooldownCts = new CancellationTokenSource();
                    WeaponAbilityCooldown(_cooldownCts.Token).Forget();
                }
            }
        }

        /// <summary>
        /// Handles the cooldown timer after the ability has completed.
        /// </summary>
        /// <param name="token">Cancellation token for the cooldown task.</param>
        public override async UniTask WeaponAbilityCooldown(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_weaponConfig.AbilityCooldown), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// Initializes the Staff special ability with the target transform.
        /// </summary>
        /// <param name="target">The player transform to center the meteor shower around.</param>
        public override void Initialize(Transform target)
        {
            _target = target;
        }

        /// <summary>
        /// Loads the meteor prefab asset using Addressables.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The loaded meteor prefab GameObject.</returns>
        private async UniTask<GameObject> LoadMeteorPrefabAsyncAddress(CancellationToken token)
        {
            if (_meteorPrefabReference.Asset != null)
            {
                return _meteorPrefabReference.Asset as GameObject;
            }

            AsyncOperationHandle<GameObject> handle = _meteorPrefabReference.LoadAssetAsync<GameObject>();
            await handle.ToUniTask(cancellationToken: token);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            return null;
        }

        /// <summary>
        /// Loops for the ability's duration, spawning meteors at regular intervals.
        /// </summary>
        /// <param name="meteorPrefab">The meteor prefab to instantiate.</param>
        /// <param name="token">Cancellation token.</param>
        private async UniTask SpawnMeteorsLoopAsync(GameObject meteorPrefab, CancellationToken token)
        {
            float duration = _weaponConfig.AbilityDuration;
            float startTime = Time.time;
            float lastSpawnTime = 0f;

            while (Time.time - startTime < duration && !token.IsCancellationRequested)
            {
                if (Time.time - lastSpawnTime >= SpawnInterval)
                {
                    lastSpawnTime = Time.time;
                    for (int spawnIndex = 0; spawnIndex < MeteorsPerSpawn; spawnIndex++)
                    {
                        SpawnMeteorAtRandomPosition(meteorPrefab);
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        /// <summary>
        /// Instantiates a single meteor at a random position within the defined radius.
        /// </summary>
        /// <param name="meteorPrefab">The meteor prefab to instantiate.</param>
        private void SpawnMeteorAtRandomPosition(GameObject meteorPrefab)
        {
            if (_target == null) return;

            Vector2 randomCirclePoint = Random.insideUnitCircle * MeteorRadius;
            Vector3 landingPosition = _target.position + new Vector3(randomCirclePoint.x, randomCirclePoint.y, 0f);

            GameObject meteor = Instantiate(meteorPrefab, landingPosition, Quaternion.identity, null);
            meteor.transform.SetParent(null); // Ensure the meteor is unparented from player hierarchy

            var meteorComponent = meteor.GetComponentInChildren<Meteor>(true);
            if (meteorComponent != null)
            {
                meteorComponent.Initialize(_weaponConfig, _percentageIncrease, landingPosition);
            }
            else
            {
                var handler = meteor.AddComponent<Meteor>();
                handler.Initialize(_weaponConfig, _percentageIncrease, landingPosition);
            }
        }
    }

    /// <summary>
    /// Helper class to handle damage application for spawned meteors using Staff weapon stats.
    /// </summary>
    public class StaffAbilityDamageHandler : MonoBehaviour
    {
        private WeaponConfig _config;
        private float _percentageIncrease;

        /// <summary>
        /// Initializes the damage handler with config and multiplier.
        /// </summary>
        public void Initialize(WeaponConfig config, float percentage)
        {
            _config = config;
            _percentageIncrease = percentage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            {
                if (_config.Afflictions != null)
                {
                    foreach (var affliction in _config.Afflictions)
                    {
                        enemy.ApplyAffliction(affliction);
                    }
                }

                float damage = _config.WeaponDamage * _percentageIncrease;
                enemy.TakeDamage(damage, transform.position, _config.WeaponKnockback, DamageSourceInfo.FromWeapon(WeaponClass.Staff));
            }
        }
    }
}
