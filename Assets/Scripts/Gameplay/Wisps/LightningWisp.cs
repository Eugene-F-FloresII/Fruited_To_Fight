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
    public class LightningWisp : WispProjectile
    {
        [Header("Lightning Wisp Settings")]
        [SerializeField] private int _maxJumps;
        [SerializeField] private float _jumpRadius;
        [SerializeField] private float _jumpDelay = 0.2f;
        [SerializeField] private GameObject _lightningVisualPrefab;

        private CancellationTokenSource _cts;
        private ObjectPool<GameObject> _visualPool;

        private void Awake()
        {
            _visualPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_lightningVisualPrefab),
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
            _circleCollider2D.radius = _wispConfig.Range;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
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

        public override void FireProjectile(EnemyController enemy)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            EnemyAlreadyHit.Clear();
            
            LightningBounce(transform.position, enemy, _maxJumps, _cts.Token);
        }

        private EnemyController GetNextTarget(EnemyController currentTarget)
        {
            return EnemyInRange
                .Where(e => e != null && e.gameObject.activeInHierarchy && !EnemyAlreadyHit.Contains(e))
                .Where(e => Vector2.Distance(currentTarget.transform.position, e.transform.position) <= _jumpRadius)
                .OrderBy(e => Vector2.Distance(currentTarget.transform.position, e.transform.position))
                .FirstOrDefault();
        }

        private void LightningBounce(Vector3 startPos, EnemyController currentTarget, int jumpsLeft, CancellationToken token)
        {
            if (currentTarget == null || token.IsCancellationRequested) return;
            
            EnemyAlreadyHit.Add(currentTarget);

            if (_wispConfig != null)
            {
                if (_wispConfig.Afflictions != null)
                {
                    foreach (var affliction in _wispConfig.Afflictions)
                    {
                        currentTarget.ApplyAffliction(affliction);
                    }
                }

                currentTarget.TakeDamage(_wispConfig.Damage, DamageSourceInfo.FromWisp(WispType.Lightning));
            }

            // Get visual from pool and set it up
            GameObject visual = _visualPool.Get();
            visual.transform.position = startPos;
            Vector2 direction = (Vector2)currentTarget.transform.position - (Vector2)startPos;
            visual.transform.right = direction;
            visual.transform.localScale = new Vector3(direction.magnitude, 1f, 1f);
            
            // Handle visual despawn independently
            DespawnVisual(visual, token).Forget();

            if (jumpsLeft <= 0) return;

            EnemyController nextTarget = GetNextTarget(currentTarget);
            if (nextTarget != null)
            {
                LightningBounce(currentTarget.transform.position, nextTarget, jumpsLeft - 1, token);
            }
        }

        private async UniTask DespawnVisual(GameObject visual, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_jumpDelay), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                // Task canceled, just clean up
            }
            finally
            {
                if (visual != null)
                {
                    _visualPool.Release(visual);
                }
            }
        }
    }
}
