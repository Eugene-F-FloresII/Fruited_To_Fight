using System;
using System.Threading;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Weapons;
using Obvious.Soap;
using Shared.Events;
using UnityEngine.AddressableAssets;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Config")]
        [SerializeField] private AssetReferenceT<EnemyConfig> _enemyConfigReference;
        
        [Header("Enemy References")]
        [SerializeField] private DefendingController _defendingController;
        [SerializeField] private Animator _animator;
        
        [Header("Material References")]
        [SerializeField] protected Material _hitMaterial;
        [SerializeField] protected Material _defaultMaterial;

        [Header("SFX clips")] 
        [SerializeField] private AudioClip _hitAudioClip;
        [SerializeField] private AudioClip _deathAudioClip;
        
        
        protected SpriteRenderer SpriteRenderer;
        private EnemyConfig _enemyConfig;
        private Rigidbody2D _enemyRb;
        private Vector2 _projectileDirection;
        private Vector2 _knockbackVelocity;
        
        private bool _isKnockedBack;
        private float _currentHealth;
        private float _currentDamage;
        private float _currentSpeed;
        private float _currentAttackSpeed;
        private Vector2 _playerPosX;
        private Vector2 _playerPosY;
        private float _currentKnockbackForce;

        private CancellationTokenSource _hitEffectCts;
        private CancellationTokenSource _knockbackCts;
        
        private readonly string _velocityX = "VelocityX";
        private readonly string _velocityY = "VelocityY";

        private void Awake()
        {
            CacheComponents();
            LoadEnemyConfigAsync().Forget();
        }

        private void OnEnable()
        {
            ResetStatsFromConfig();
        }

        private void OnDisable()
        {
            DisposeTokens();

            if (SpriteRenderer != null)
                SpriteRenderer.material = _defaultMaterial;
        }

        private void OnDestroy()
        {
            DisposeTokens();
            if(_enemyConfigReference.IsValid())
            {
                _enemyConfigReference.ReleaseAsset();
            }
        }

        private void FixedUpdate()
        {
            ChasePlayer();
        }

        public void KillEnemy()
        {
            Events_Sound.PlaySound?.Invoke(_deathAudioClip);
            Events_Seed.OnEnemyDeath?.Invoke(transform);
            Events_Enemy.OnEnemyDeath?.Invoke();
            gameObject.SetActive(false);
        }

        public void TakeDamage(float damage, ProjectileWeapon projectile)
        {
            _projectileDirection = (transform.position - projectile.transform.position).normalized;
            
            if (_knockbackCts != null)
            {
                _knockbackCts.Cancel();
                _knockbackCts.Dispose();
                _knockbackCts = null;
            }
            _knockbackCts =  new CancellationTokenSource();
            
            EnemyKnockBack(_projectileDirection, projectile.GetWeaponKnockback(), 0.3f, _knockbackCts.Token).Forget();
            
            _currentHealth -= damage;
            Events_Enemy.OnEnemyHit?.Invoke(transform.position, Mathf.RoundToInt(damage));
            
            if (_hitEffectCts != null)
            {
                _hitEffectCts.Cancel();
                _hitEffectCts.Dispose();
                _hitEffectCts = null;
            }
            _hitEffectCts = new CancellationTokenSource();
            HitEffect(_hitEffectCts.Token).Forget();
            
            Events_Sound.PlaySound?.Invoke(_hitAudioClip);
            
            if (_currentHealth <= 0)
            {
                KillEnemy();
                
            }
        }

        public float GetKnockBackForce()
        {
            return _currentKnockbackForce;
        }

        public int GotHitByEnemy()
        {
            if (_enemyConfig == null) return 0;
            return (int)_enemyConfig.EnemyDamage;
        }

        public void ApplyRuntimeStats(EnemyRuntimeStats runtimeStats)
        {
            _currentHealth = runtimeStats.Health;
            _currentDamage = runtimeStats.Damage;
            _currentSpeed = runtimeStats.MoveSpeed;
            _currentAttackSpeed = runtimeStats.AttackSpeed;
            _currentKnockbackForce = runtimeStats.KnockbackForce;
        }

        public void InitializePlayer(DefendingController defendingController) =>  _defendingController = defendingController; 

        private void ChasePlayer()
        {
            if (_isKnockedBack)
            {
                // Decay knockback over time
                _knockbackVelocity = Vector2.Lerp(_knockbackVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
                transform.position += (Vector3)_knockbackVelocity * Time.fixedDeltaTime;
                return;
            }
            
            if (_defendingController == null) return;

            _playerPosX.x = _defendingController.transform.position.x;
            _playerPosY.y = _defendingController.transform.position.y;

            Vector2 FinalPos = new Vector2(_playerPosX.x, _playerPosY.y);
            Vector2 EnemyPos = new Vector2(transform.position.x, transform.position.y);

            Vector2 direction = FinalPos - EnemyPos;
            Vector2 normalizedDirection = direction.normalized;
            
            _animator.SetFloat(_velocityX, normalizedDirection.x);
            _animator.SetFloat(_velocityY, normalizedDirection.y);
            
            gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, _defendingController.transform.position, _currentSpeed * Time.deltaTime);
        }

        private void ResetStatsFromConfig()
        {
            if (_enemyConfig == null)
            {
                return;
            }

            _currentHealth = _enemyConfig.EnemyHealth;
            _currentDamage = _enemyConfig.EnemyDamage;
            _currentKnockbackForce = _enemyConfig.EnemyKnockbackForce;
            _currentSpeed = _enemyConfig.EnemyMoveSpeed;
            _currentAttackSpeed = _enemyConfig.EnemyAtkSpeed;
        }

        private void CacheComponents()
        {
            _enemyRb = GetComponent<Rigidbody2D>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void DisposeTokens()
        {
            if (_hitEffectCts != null)
            {
                _hitEffectCts.Cancel();
                _hitEffectCts.Dispose();
                _hitEffectCts = null;
            }

            if (_knockbackCts != null)
            {
                _knockbackCts.Cancel();
                _knockbackCts.Dispose();
                _knockbackCts = null;
            }
        }
        
        protected virtual async UniTask HitEffect(CancellationToken token)
        {
            if (_hitMaterial != null && SpriteRenderer != null)
            {
                try
                {
                    SpriteRenderer.material = _hitMaterial;
                    await UniTask.Delay(150, cancellationToken: token);
                    if (SpriteRenderer != null)
                        SpriteRenderer.material = _defaultMaterial;
                }
                catch (OperationCanceledException)
                {
                    // Expected on cancellation
                }
                catch (Exception e) when (e is MissingReferenceException || e is ObjectDisposedException)
                {
                    Debug.Log("Entity Dead");
                }
            }
        }
        
        private async UniTaskVoid LoadEnemyConfigAsync()
        {
            _enemyConfig = await _enemyConfigReference.LoadAssetAsync<EnemyConfig>().ToUniTask();

            ResetStatsFromConfig();
        }
        
        private async UniTask EnemyKnockBack(Vector2 direction, float force, float duration, CancellationToken token)
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
                // Expected on cancellation
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