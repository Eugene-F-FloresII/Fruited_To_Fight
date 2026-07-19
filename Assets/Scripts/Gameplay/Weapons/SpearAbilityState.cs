using System;
using System.Collections.Generic;
using System.Threading;
using Collection;
using Controllers;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Shared.Enums;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Handles the Spear special ability: a single-burst spike eruption at the feet of all enemies in a radius.
    /// Spawns warning circle indicators 0.25s prior to spike eruption.
    /// </summary>
    public class SpearAbilityState : WeaponAbilityState
    {
        [Header("Spear Ability Settings")]
        [SerializeField] private AssetReferenceGameObject _warningCirclePrefabReference;
        [SerializeField] private AssetReferenceGameObject _spikePrefabReference;
        [SerializeField] private Transform _target; // Player's Transform
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private float _spikeRadius = 1.0f;

        private float AbilityRadius => _weaponConfig != null ? _weaponConfig.AbilityRadius : 5.0f;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SpearAbilityState>();
            
            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _abilityCts = null;

            _cooldownCts?.Cancel();
            _cooldownCts?.Dispose();
            _cooldownCts = null;
            
            if (_warningCirclePrefabReference != null && _warningCirclePrefabReference.IsValid())
            {
                _warningCirclePrefabReference.ReleaseAsset();
            }
            if (_spikePrefabReference != null && _spikePrefabReference.IsValid())
            {
                _spikePrefabReference.ReleaseAsset();
            }
        }

        public override void UseWeaponAbility()
        {
            if (_weaponConfig == null)
            {
                Debug.LogError("WeaponConfig is NULL on SpearAbilityState!", this);
                return;
            }

            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _abilityCts = new CancellationTokenSource();
            
            UseWeaponAbilityAsync(_abilityCts.Token).Forget();
        }

        public override async UniTask UseWeaponAbilityAsync(CancellationToken token)
        {
            try
            {
                if (_warningCirclePrefabReference == null || _spikePrefabReference == null)
                {
                    Debug.LogError("[SpearAbility] Warning circle or Spike prefab reference is null!");
                    return;
                }

                // Load assets in parallel
                var (warningCirclePrefab, spikePrefab) = await UniTask.WhenAll(
                    LoadPrefabAsync(_warningCirclePrefabReference, token),
                    LoadPrefabAsync(_spikePrefabReference, token)
                );

                if (warningCirclePrefab == null || spikePrefab == null)
                {
                    Debug.LogError("[SpearAbility] Failed to load ability prefabs!");
                    return;
                }

                SpawnSpikesAtEnemies(warningCirclePrefab, spikePrefab);
            }
            catch (OperationCanceledException)
            {
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

        public override void Initialize(Transform target)
        {
            _target = target;
        }

        private async UniTask<GameObject> LoadPrefabAsync(AssetReferenceGameObject assetRef, CancellationToken token)
        {
            if (assetRef.Asset != null)
            {
                return assetRef.Asset as GameObject;
            }

            AsyncOperationHandle<GameObject> handle = assetRef.LoadAssetAsync<GameObject>();
            await handle.ToUniTask(cancellationToken: token);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            return null;
        }

        private void SpawnSpikesAtEnemies(GameObject warningCirclePrefab, GameObject spikePrefab)
        {
            if (_target == null) return;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(_target.position, AbilityRadius, _enemyLayerMask);
            var uniqueEnemies = new HashSet<EnemyController>();

            foreach (var collider in colliders)
            {
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy == null)
                {
                    enemy = collider.GetComponentInParent<EnemyController>();
                }

                if (enemy != null && uniqueEnemies.Add(enemy))
                {
                    Vector3 spawnPos = enemy.transform.position;
                    TriggerSpikeSequence(warningCirclePrefab, spikePrefab, spawnPos).Forget();
                }
            }
        }

        private async UniTaskVoid TriggerSpikeSequence(GameObject warningCirclePrefab, GameObject spikePrefab, Vector3 position)
        {
            float warningDuration = 0.25f;

            if (warningCirclePrefab != null)
            {
                GameObject circleObj = Instantiate(warningCirclePrefab, position, Quaternion.identity);
                if (circleObj.TryGetComponent(out AbilityWarningCircle warningCircle))
                {
                    warningCircle.Initialize(_spikeRadius, warningDuration);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(warningDuration));

            if (this == null || spikePrefab == null) return;

            GameObject spikeObj = Instantiate(spikePrefab, position, Quaternion.identity);
            if (spikeObj.TryGetComponent(out SpearSpike spike))
            {
                spike.Initialize(_weaponConfig, _percentageIncrease, _spikeRadius);
            }
        }
    }
}
