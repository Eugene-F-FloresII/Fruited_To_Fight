using System.Threading;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Config")]
        [SerializeField] private EnemyConfig _enemyConfig;
        
        [Header("Enemy References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Animator _animator;
        
        [Header("Material References")]
        [SerializeField] private Material _hitMaterial;
        [SerializeField] private Material _defaultMaterial;
        
        private SpriteRenderer _spriteRenderer;
        private float _currentHealth;
        private float _currentSpeed;
        private float _playerPosX;
        private float _playerPosY;

        private CancellationTokenSource _hitEffectCts;
        
        private readonly string _velocityX = "VelocityX";
        private readonly string _velocityY = "VelocityY";

        private void OnEnable()
        {
            UpdateEnemyStats();
        }

        private void OnDisable()
        {
            _hitEffectCts?.Cancel();
            _hitEffectCts?.Dispose();
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
            _currentSpeed = _enemyConfig.EnemyMoveSpeed;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private async UniTask HitEffect(CancellationToken token)
        {
            if (_hitMaterial != null)
            {
                try
                {
                    _spriteRenderer.material = _hitMaterial;
                    await UniTask.Delay(150, cancellationToken: token);
                    _spriteRenderer.material = _defaultMaterial;
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
        
        
    }

}
