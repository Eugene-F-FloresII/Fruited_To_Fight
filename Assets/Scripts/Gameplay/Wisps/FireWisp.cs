using System;
using System.Linq;
using System.Threading;
using Controllers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Shared.Events;
using Shared.Enums;

namespace Gameplay.Wisps
{
    public class FireWisp : WispProjectile
    {
        [Header("Fire Wisp Settings")]
        [SerializeField, Range(0.1f, 1f)] private float _aoeRadiusPercentage = 0.2f;
        [SerializeField] private GameObject _meteorVisualPrefab;
        [SerializeField] private Vector2 _spawnOffset = new Vector2(3f, 5f);
        
        private CancellationTokenSource _cts;
        private ObjectPool<GameObject> _meteorPool;
        private static readonly Collider2D[] _overlapResults = new Collider2D[20];
        private readonly System.Collections.Generic.List<ActiveMeteor> _activeMeteors = new System.Collections.Generic.List<ActiveMeteor>();

        private class ActiveMeteor
        {
            public GameObject MeteorObj;
            public Vector3 TargetPos;
        }

        private void Awake()
        {
            _meteorPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_meteorVisualPrefab),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 20
            );
        }

        private void Start()
        {
            if (_circleCollider2D != null && _wispConfig != null)
            {
                _circleCollider2D.radius = _wispConfig.Range;
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTask AnimateMeteorAsync(GameObject meteor, Vector3 targetPos, CancellationToken token)
        {
            try
            {
                Vector3 startPos = meteor.transform.position;
                float duration = 1f; // Default duration
                
                // Calculate duration based on distance and ProjectileSpeed (avoiding divide by zero)
                if (_wispConfig != null && _wispConfig.ProjectileSpeed > 0)
                {
                    float distance = Vector3.Distance(startPos, targetPos);
                    duration = distance / _wispConfig.ProjectileSpeed;
                }
                
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    if (token.IsCancellationRequested) return;

                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    
                    meteor.transform.position = Vector3.Lerp(startPos, targetPos, t);
                    
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                
                // Ensure it reaches exactly the target
                meteor.transform.position = targetPos;
            }
            catch (OperationCanceledException)
            {
                // Task canceled, just clean up
            }
            finally
            {
                if (meteor != null)
                {
                    _meteorPool.Release(meteor);
                }
            }
        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemyController))
            { 
                if (!EnemyInRange.Contains(enemyController))
                {
                    EnemyInRange.Add(enemyController);
                }
            }
        }

        public override void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemyController))
            {
                if (EnemyInRange.Contains(enemyController))
                {
                    EnemyInRange.Remove(enemyController);
                }
            }
        }

        public override void FireProjectile(EnemyController target)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            
            // Execute the meteor drop sequence
            DropMeteorAsync(target, _cts.Token).Forget();
        }
        
        private async UniTask DropMeteorAsync(EnemyController target, CancellationToken token)
        {
            if (target == null || token.IsCancellationRequested || _wispConfig == null) return;
            
            Vector3 targetPosition = target.transform.position;
            Vector3 spawnPosition = targetPosition + (Vector3)_spawnOffset;
            
            GameObject meteor = _meteorPool.Get();
            meteor.transform.position = spawnPosition;
            
            // Face the target
            Vector2 direction = (targetPosition - spawnPosition).normalized;
            meteor.transform.up = direction; // Assuming meteor sprite faces UP by default. Change to .right if it faces right.
            
            ActiveMeteor activeDrop = new ActiveMeteor { MeteorObj = meteor, TargetPos = targetPosition };
            _activeMeteors.Add(activeDrop);

            try
            {
                // Wait for meteor to hit the ground
                await AnimateMeteorAsync(meteor, targetPosition, token);
                
                if (token.IsCancellationRequested) return;

                // Explosion logic
                if (_wispConfig.HitEffectPrefab != null && _wispConfig.HitEffectPrefab.RuntimeKeyIsValid())
                {
                    Events_VFX.SpawnVFXEvent?.Invoke(_wispConfig.HitEffectPrefab, targetPosition, Quaternion.identity, Vector3.one, 1f);
                }

                // AoE Damage
                float aoeRadius = _wispConfig.Range * _aoeRadiusPercentage;
                
                // Use OverlapCircleNonAlloc for performance
                int hitCount = Physics2D.OverlapCircleNonAlloc(targetPosition, aoeRadius, _overlapResults);
                
                for (int i = 0; i < hitCount; i++)
                {
                    if (_overlapResults[i].TryGetComponent(out EnemyController enemy))
                    {
                        if (enemy.gameObject.activeInHierarchy)
                        {
                            if (_wispConfig.Afflictions != null)
                            {
                                foreach (var affliction in _wispConfig.Afflictions)
                                {
                                    enemy.ApplyAffliction(affliction);
                                }
                            }

                            // Apply Damage
                            enemy.TakeDamage(_wispConfig.Damage, DamageSourceInfo.FromWisp(WispType.Fire));
                        }
                    }
                }
            }
            finally
            {
                _activeMeteors.Remove(activeDrop);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_wispConfig != null)
            {
                // Draw Wisp's base attack range
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, _wispConfig.Range);
            }
        }

        private void OnDrawGizmos()
        {
            if (_wispConfig == null || _activeMeteors == null || _activeMeteors.Count == 0) return;

            float aoeRadius = _wispConfig.Range * _aoeRadiusPercentage;

            foreach (var active in _activeMeteors)
            {
                if (active.MeteorObj != null && active.MeteorObj.activeInHierarchy)
                {
                    // Draw path from meteor to target
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(active.MeteorObj.transform.position, active.TargetPos);
                    
                    // Draw AoE blast zone at the target position
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange
                    Gizmos.DrawWireSphere(active.TargetPos, aoeRadius);
                    
                    // Draw a solid sphere for the meteor itself
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(active.MeteorObj.transform.position, 0.2f);
                }
            }
        }
    }
}
