using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using Controllers;

namespace Gameplay.Powerups
{
    public abstract class PowerUp : MonoBehaviour
    {
        [SerializeField] protected PowerUpConfig _powerUpConfig;

        [Header("Attraction Settings")]
        [SerializeField] protected float _followSpeed = 5f;
        [SerializeField] protected Collider2D _collectionCollider;

        protected PlayerController _targetPlayer;
        protected Rigidbody2D _rb;
        private CancellationTokenSource _despawnCts;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void OnEnable()
        {
            _targetPlayer = null;
            PowerUpDurationAsync().Forget();
        }

        protected virtual void OnDisable()
        {
            CancelDespawnTimer();
        }

        protected virtual void FixedUpdate()
        {
            if (_targetPlayer == null) return;

            Vector2 currentPosition = _rb != null ? _rb.position : (Vector2)transform.position;
            Vector2 targetPosition = _targetPlayer.transform.position;
            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, _followSpeed * Time.fixedDeltaTime);

            if (_rb != null)
            {
                _rb.MovePosition(nextPosition);
            }
            else
            {
                transform.position = nextPosition;
            }
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                if (_targetPlayer == null)
                {
                    StartFollowing(player);
                }

                if (_collectionCollider != null)
                {
                    if (other.IsTouching(_collectionCollider))
                    {
                        Collect();
                    }
                }
                else
                {
                    // Fallback: collect if distance is very small
                    if (Vector2.Distance(transform.position, other.transform.position) < 0.3f)
                    {
                        Collect();
                    }
                }
            }
        }

        public virtual void StartFollowing(PlayerController player)
        {
            _targetPlayer = player;
            CancelDespawnTimer();
        }

        protected virtual void Collect()
        {
            UsePowerUp();
            gameObject.SetActive(false);
        }

        public virtual void UsePowerUp()
        {
            throw new System.NotImplementedException();
        }

        public virtual async UniTask PowerUpDurationAsync()
        {
            CancelDespawnTimer();
            _despawnCts = new CancellationTokenSource();

            try
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _despawnCts.Token, 
                    this.GetCancellationTokenOnDestroy()
                );

                await UniTask.Delay(TimeSpan.FromSeconds(_powerUpConfig.DespawnDuration), cancellationToken: linkedCts.Token);
                
                if (_targetPlayer == null)
                {
                    gameObject.SetActive(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelDespawnTimer()
        {
            if (_despawnCts != null)
            {
                _despawnCts.Cancel();
                _despawnCts.Dispose();
                _despawnCts = null;
            }
        }
    }
}
