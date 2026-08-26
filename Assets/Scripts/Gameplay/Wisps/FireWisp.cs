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
        [SerializeField] private float _arcHeightMultiplier = 0.5f;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private UnityEngine.AddressableAssets.AssetReferenceGameObject _explosionPrefab;
        [SerializeField] private float _explosionDuration = 0.5f;
        [SerializeField] private float _explosionScaleMultiplier = 1.25f;
        
        private CancellationTokenSource _cts;
        private ObjectPool<GameObject> _projectilePool;
        private static readonly Collider2D[] _overlapResults = new Collider2D[20];
        private readonly System.Collections.Generic.List<ActiveProjectile> _activeProjectiles = new System.Collections.Generic.List<ActiveProjectile>();

        private class ActiveProjectile
        {
            public GameObject ProjectileObj;
            public Vector3 StartPos;
            public Vector3 TargetPos;
            public float Distance;
        }

        private void Awake()
        {
            _projectilePool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_projectilePrefab),
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

        private async UniTask AnimateProjectileAsync(GameObject projectile, Vector3 startPos, Vector3 targetPos, float distance, CancellationToken token)
        {
            try
            {
                float duration = 1f; // Default duration
                
                // Calculate duration based on distance and ProjectileSpeed (avoiding divide by zero)
                if (_wispConfig != null && _wispConfig.ProjectileSpeed > 0)
                {
                    duration = distance / _wispConfig.ProjectileSpeed;
                }
                
                float elapsed = 0f;
                Vector3 previousPos = startPos;

                while (elapsed < duration)
                {
                    if (token.IsCancellationRequested) return;

                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    
                    // Linear lerp for base position
                    Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
                    
                    // Add arc height using a sine wave
                    float arc = Mathf.Sin(t * Mathf.PI) * distance * _arcHeightMultiplier;
                    currentPos.y += arc;
                    
                    projectile.transform.position = currentPos;
                    
                    // Rotate to face travel direction
                    Vector2 direction = (currentPos - previousPos).normalized;
                    if (direction != Vector2.zero)
                    {
                        projectile.transform.right = direction; // Assuming fireball sprite faces RIGHT by default.
                    }
                    
                    previousPos = currentPos;
                    
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                
                // Ensure it reaches exactly the target
                projectile.transform.position = targetPos;
            }
            catch (OperationCanceledException)
            {
                // Task canceled, just clean up
            }
            finally
            {
                if (projectile != null)
                {
                    _projectilePool.Release(projectile);
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
            
            // Execute the mortar firing sequence
            FireMortarAsync(target, _cts.Token).Forget();
        }
        
        private async UniTask FireMortarAsync(EnemyController target, CancellationToken token)
        {
            if (target == null || token.IsCancellationRequested || _wispConfig == null) return;
            
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = target.transform.position;
            float distance = Vector3.Distance(startPosition, targetPosition);
            
            GameObject projectile = _projectilePool.Get();
            projectile.transform.position = startPosition;
            
            ActiveProjectile activeDrop = new ActiveProjectile { 
                ProjectileObj = projectile, 
                StartPos = startPosition,
                TargetPos = targetPosition,
                Distance = distance
            };
            
            _activeProjectiles.Add(activeDrop);

            try
            {
                // Wait for mortar to hit the ground
                await AnimateProjectileAsync(projectile, startPosition, targetPosition, distance, token);
                
                if (token.IsCancellationRequested) return;

                // AoE Explosion Radius
                float aoeRadius = _wispConfig.Range * _aoeRadiusPercentage;
                
                // Spawn giant Mortar Explosion VFX
                if (_explosionPrefab != null && _explosionPrefab.RuntimeKeyIsValid())
                {
                    Vector3 explosionScale = Vector3.one * (aoeRadius * 2f * _explosionScaleMultiplier);
                    Events_VFX.SpawnVFXEvent?.Invoke(_explosionPrefab, targetPosition, Quaternion.identity, explosionScale, _explosionDuration);
                }

                // Use OverlapCircleNonAlloc for performance
                int hitCount = Physics2D.OverlapCircleNonAlloc(targetPosition, aoeRadius, _overlapResults);
                
                for (int i = 0; i < hitCount; i++)
                {
                    if (_overlapResults[i].TryGetComponent(out EnemyController enemy))
                    {
                        if (enemy.gameObject.activeInHierarchy)
                        {
                            // Spawn tiny Enemy Hit Spark directly on the hit enemy
                            if (_wispConfig.HitEffectPrefab != null && _wispConfig.HitEffectPrefab.RuntimeKeyIsValid())
                            {
                                Events_VFX.SpawnVFXEvent?.Invoke(_wispConfig.HitEffectPrefab, enemy.transform.position, Quaternion.identity, Vector3.one, 1f);
                            }

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
                _activeProjectiles.Remove(activeDrop);
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
            if (_wispConfig == null || _activeProjectiles == null || _activeProjectiles.Count == 0) return;

            float aoeRadius = _wispConfig.Range * _aoeRadiusPercentage;

            foreach (var active in _activeProjectiles)
            {
                if (active.ProjectileObj != null && active.ProjectileObj.activeInHierarchy)
                {
                    // Draw AoE blast zone at the target position
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange
                    Gizmos.DrawWireSphere(active.TargetPos, aoeRadius);
                    
                    // Draw a solid sphere for the projectile itself
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(active.ProjectileObj.transform.position, 0.2f);
                    
                    // Draw the projected arc trajectory
                    Gizmos.color = Color.red;
                    int segments = 20;
                    Vector3 previousPoint = active.StartPos;
                    for (int i = 1; i <= segments; i++)
                    {
                        float t = i / (float)segments;
                        Vector3 point = Vector3.Lerp(active.StartPos, active.TargetPos, t);
                        float arc = Mathf.Sin(t * Mathf.PI) * active.Distance * _arcHeightMultiplier;
                        point.y += arc;
                        
                        Gizmos.DrawLine(previousPoint, point);
                        previousPoint = point;
                    }
                }
            }
        }
    }
}
