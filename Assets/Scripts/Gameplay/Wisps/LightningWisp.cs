using System;
using System.Linq;
using System.Threading;
using Controllers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Wisps
{
    [RequireComponent(typeof(LineRenderer))]
    public class LightningWisp : WispProjectile
    {
        [Header("Lightning Wisp Settings")]
        [SerializeField] private int _maxJumps;
        [SerializeField] private float _jumpRadius;
        [SerializeField] private float _jumpDelay = 0.2f;
        [SerializeField] private LineRenderer _lineRenderer;

        private CancellationTokenSource _cts;

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
            _lineRenderer.positionCount = 1;
            _lineRenderer.SetPosition(0, transform.position);
            
            LightningBounce(enemy, _maxJumps, _cts.Token).Forget();
        }

        private EnemyController GetNextTarget(EnemyController currentTarget)
        {
            return EnemyInRange
                .Where(e => e != null && e.gameObject.activeInHierarchy && !EnemyAlreadyHit.Contains(e))
                .Where(e => Vector2.Distance(currentTarget.transform.position, e.transform.position) <= _jumpRadius)
                .OrderBy(e => Vector2.Distance(currentTarget.transform.position, e.transform.position))
                .FirstOrDefault();
        }

        private async UniTask LightningBounce(EnemyController currentTarget, int jumpsLeft, CancellationToken token)
        {
            if (currentTarget == null || token.IsCancellationRequested) return;
            
            EnemyAlreadyHit.Add(currentTarget);

            if (_wispConfig != null)
            {
                currentTarget.TakeDamage(_wispConfig.Damage);

                if (_wispConfig.Afflictions != null)
                {
                    foreach (var affliction in _wispConfig.Afflictions)
                    {
                        currentTarget.ApplyAffliction(affliction);
                    }
                }
            }

            // Always update the root position to match the wisp's current position to prevent stretching
            _lineRenderer.SetPosition(0, transform.position);

            int pointIndex = _lineRenderer.positionCount;
            _lineRenderer.positionCount = pointIndex + 1;
            _lineRenderer.SetPosition(pointIndex, currentTarget.transform.position);
            
            if (jumpsLeft <= 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_jumpDelay), cancellationToken: token).SuppressCancellationThrow();
                if (token.IsCancellationRequested) return;
                
                _lineRenderer.positionCount = 0;
                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_jumpDelay), cancellationToken: token).SuppressCancellationThrow();
            if (token.IsCancellationRequested) return;

            EnemyController nextTarget = GetNextTarget(currentTarget);
            if (nextTarget != null)
            {
                await LightningBounce(nextTarget, jumpsLeft - 1, token);
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_jumpDelay), cancellationToken: token).SuppressCancellationThrow();
                if (token.IsCancellationRequested) return;
                _lineRenderer.positionCount = 0;
            }
        }
    }
}
