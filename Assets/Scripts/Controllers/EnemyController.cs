using System;
using System.Threading;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
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
        
        
        private float _currentHealth;
        private float _currentSpeed;
        private float _playerPosX;
        private float _playerPosY;
        private float _currentKnockbackForce;

        private CancellationTokenSource _hitEffectCts;
        
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
            _hitEffectCts?.Cancel();
            _hitEffectCts?.Dispose();

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

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            
            _hitEffectCts = new CancellationTokenSource();
            HitEffect(_hitEffectCts.Token).Forget();

            if (_currentHealth <= 0)
            {
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
            SpriteRenderer = GetComponent<SpriteRenderer>();
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
        
        
    }

}
