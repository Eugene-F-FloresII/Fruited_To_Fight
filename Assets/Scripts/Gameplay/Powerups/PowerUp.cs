using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using Controllers;
using Collection;

namespace Gameplay.Powerups
{
    public abstract class PowerUp : MonoBehaviour
    {
        [SerializeField] protected PowerUpConfig _powerUpConfig;

        public PowerUpConfig PowerUpConfig => _powerUpConfig;

        protected PlayerController _playerController;
        protected Rigidbody2D _rb;
        private CancellationTokenSource _despawnCts;

        private float _followSpeed;
        private float _attractionRadius;
        private bool _isInitialized;
        private bool _isInitializing;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void Start()
        {
            EnsureInitialized().Forget();
        }

        protected virtual void OnEnable()
        {
            EnsureInitialized().Forget();
            PowerUpDurationAsync().Forget();
        }

        protected virtual void OnDisable()
        {
            CancelDespawnTimer();
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        protected virtual void FixedUpdate()
        {
            FollowPlayer();
        }

        private async UniTask EnsureInitialized()
        {
            if (_isInitialized || _isInitializing)
            {
                return;
            }

            _isInitializing = true;

            try
            {
                _playerController = ServiceLocator.Get<PlayerController>();
                if (_powerUpConfig != null)
                {
                    _followSpeed = _powerUpConfig.FollowSpeed;
                    _attractionRadius = _powerUpConfig.AttractionRadius;
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

        protected virtual void FollowPlayer()
        {
            if (!_isInitialized || _playerController == null)
            {
                return;
            }

            Vector2 currentPosition = _rb != null ? _rb.position : (Vector2)transform.position;
            Vector2 targetPosition = _playerController.transform.position;
            float sqrDistance = (targetPosition - currentPosition).sqrMagnitude;
            float sqrRadius = _attractionRadius * _attractionRadius;

            if (sqrDistance > sqrRadius)
            {
                return;
            }

            // Cancel despawn timer once the player is in range/attracting the power-up
            CancelDespawnTimer();

            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, _followSpeed * Time.fixedDeltaTime);

            if (_rb != null)
            {
                _rb.MovePosition(nextPosition);
                return;
            }

            transform.position = nextPosition;
        }

        public virtual void Collect()
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
                
                gameObject.SetActive(false);
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
