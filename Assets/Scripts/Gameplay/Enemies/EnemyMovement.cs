using System;
using System.Threading;
using Controllers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Enemies
{
    public class EnemyMovement : MonoBehaviour
    {
        private Rigidbody2D _enemyRb;
        private PlayerController _playerController;
        private EnemyVisuals _enemyVisuals;
        
        private float _currentSpeed;
        private bool _isKnockedBack;
        private bool _isFrozen;
        private Vector2 _knockbackVelocity;
        
        private CancellationTokenSource _knockbackCts;

        private void Awake()
        {
            _enemyRb = GetComponent<Rigidbody2D>();
            _enemyVisuals = GetComponent<EnemyVisuals>();
        }

        private void OnEnable()
        {
            _isFrozen = false;
        }

        private void OnDisable()
        {
            if (_knockbackCts != null)
            {
                _knockbackCts.Cancel();
                _knockbackCts.Dispose();
                _knockbackCts = null;
            }
        }

        private void FixedUpdate()
        {
            ChasePlayer();
        }

        public void Initialize(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void SetSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public void SetFrozen(bool frozen)
        {
            _isFrozen = frozen;
        }

        private void ChasePlayer()
        {
            if (_isFrozen) return;

            if (_isKnockedBack)
            {
                _knockbackVelocity = Vector2.Lerp(_knockbackVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
                transform.position += (Vector3)_knockbackVelocity * Time.fixedDeltaTime;
                return;
            }
            
            if (_playerController == null) return;

            float playerPosX = _playerController.transform.position.x;
            float playerPosY = _playerController.transform.position.y;
            
            if (_enemyVisuals != null)
            {
                _enemyVisuals.SetAnimationVelocity(playerPosX, playerPosY);
            }
            
            transform.position = Vector2.MoveTowards(transform.position, _playerController.transform.position, _currentSpeed * Time.deltaTime);
        }

        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            if (_knockbackCts != null)
            {
                _knockbackCts.Cancel();
                _knockbackCts.Dispose();
            }
            _knockbackCts = new CancellationTokenSource();
            
            EnemyKnockBackAsync(direction, force, duration, _knockbackCts.Token).Forget();
        }

        private async UniTask EnemyKnockBackAsync(Vector2 direction, float force, float duration, CancellationToken token)
        {
            try
            {
                if (_enemyRb != null)
                {
                    _enemyRb.linearVelocity = Vector2.zero;
                    _enemyRb.AddForce(direction * force, ForceMode2D.Impulse);
                }
                _isKnockedBack = true;
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isKnockedBack = false;
                if (_enemyRb != null)
                {
                    _enemyRb.linearVelocity = Vector2.zero;
                }
            }
        }
    }
}
