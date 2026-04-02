using System;
using System.Threading;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Weapons;
using Shared.Events;
using UnityEngine.AddressableAssets;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Config")]
        [SerializeField] private AssetReferenceT<EnemyConfig> _enemyConfigReference;
        
        [Header("Enemy References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Animator _animator;
        
        [Header("Material References")]
        [SerializeField] protected Material _hitMaterial;
        [SerializeField] protected Material _defaultMaterial;
        
        protected SpriteRenderer SpriteRenderer;
        private EnemyConfig _enemyConfig;
        private Rigidbody2D _enemyRb;
        private Vector2 _projectileDirection;
        private Vector2 _knockbackVelocity;
        
        private bool _isKnockedBack;
        private float _currentHealth;
        private float _currentSpeed;
        private float _playerPosX;
        private float _playerPosY;
        private float _currentKnockbackForce;

        private CancellationTokenSource _hitEffectCts;
        private CancellationTokenSource _knockbackCts;
        
        private readonly string _velocityX = "VelocityX";
        private readonly string _velocityY = "VelocityY";

        private void Awake()
        {
            LoadEnemyConfigAsync().Forget();
        }

        private void OnEnable()
        {
            UpdateEnemyStats();
        }

        private void OnDisable()
        {
            DisposeTokens();

            SpriteRenderer.material = _defaultMaterial;
        }

        private void OnDestroy()
        {
            if(_enemyConfigReference.IsValid())
            {
                _enemyConfigReference.ReleaseAsset();
            }
        }

        private void FixedUpdate()
        {
            ChasePlayer();
        }

        public void TakeDamage(float damage, ProjectileWeapon projectile)
        {
            _projectileDirection = (transform.position - projectile.transform.position).normalized;
            
            _knockbackCts =  new CancellationTokenSource();
            
            EnemyKnockBack(_projectileDirection, projectile.GetWeaponKnockback(), 0.3f, _knockbackCts.Token).Forget();
            
            _currentHealth -= damage;
            
            _hitEffectCts = new CancellationTokenSource();
            HitEffect(_hitEffectCts.Token).Forget();

            if (_currentHealth <= 0)
            {
                Events_Seed.OnEnemyDeath?.Invoke(transform);
                gameObject.SetActive(false);
            }
        }

        public float GetKnockBackForce()
        {
            return _currentKnockbackForce;
        }

        public void InitializePlayer(PlayerController playerController) =>  _playerController = playerController; 

        private void ChasePlayer()
        {
            if (_isKnockedBack)
            {
                // Decay knockback over time
                _knockbackVelocity = Vector2.Lerp(_knockbackVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
                transform.position += (Vector3)_knockbackVelocity * Time.fixedDeltaTime;
                return;
            }
            
            _playerPosX = _playerController.transform.position.x;
            _playerPosY = _playerController.transform.position.y;
            
            _animator.SetFloat(_velocityX, _playerPosX);
            _animator.SetFloat(_velocityY, _playerPosY);
            
            
            gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, _playerController.transform.position, _currentSpeed * Time.deltaTime);
        }

        private void UpdateEnemyStats()
        {
            _currentHealth = _enemyConfig.EnemyHealth;
            _currentKnockbackForce = _enemyConfig.EnemyKnockbackForce;
            _currentSpeed = _enemyConfig.EnemyMoveSpeed;
            _enemyRb = GetComponent<Rigidbody2D>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void DisposeTokens()
        {
            _hitEffectCts?.Cancel();
            _hitEffectCts?.Dispose();

            _knockbackCts?.Cancel();
            _knockbackCts?.Dispose();
        }

        protected virtual async UniTask HitEffect(CancellationToken token)
        {
            if (_hitMaterial != null)
            {
                try
                {
                    SpriteRenderer.material = _hitMaterial;
                    await UniTask.Delay(150, cancellationToken: token);
                    SpriteRenderer.material = _defaultMaterial;
                }
                catch (MissingReferenceException)
                {
                    Debug.Log("Entity Dead");
                }
                catch (TaskCanceledException)
                {
                    Debug.Log("Entity Dead");
                }
            }
        }
        
        private async UniTaskVoid LoadEnemyConfigAsync()
        {
            _enemyConfig = await _enemyConfigReference.LoadAssetAsync<EnemyConfig>().ToUniTask();

            UpdateEnemyStats();
        }
        
        private async UniTask EnemyKnockBack(Vector2 direction, float force, float duration, CancellationToken token)
        {
            try
            {
                _enemyRb.linearVelocity = Vector2.zero;
                _enemyRb.AddForce(direction * force, ForceMode2D.Impulse);
                _isKnockedBack = true;
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            }
            catch (OperationCanceledException)
            {
                Debug.Log("Enemy Knocked Back");
              
            }
            finally
            {
                _isKnockedBack = false;
                _enemyRb.linearVelocity = Vector2.zero;
            }
        }
        
        
    }

}
