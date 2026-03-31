using System.Threading;
using System.Threading.Tasks;
using Data;
using UnityEngine;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyConfig _enemyConfig;
        
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Animator _animator;
        
        [Header("Material References")]
        [SerializeField] private Material _hitMaterial;
        [SerializeField] private Material _defaultMaterial;
        
        private SpriteRenderer _spriteRenderer;
        private float _currentHealth;
        private float _currentSpeed;

        private CancellationTokenSource _hitEffectCts;
        private void Awake()
        {
            UpdateEnemyStats();
        }

        private void FixedUpdate()
        {
            ChasePlayer();
        }

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            
            HitEffect();

            if (_currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void InitializePlayer(PlayerController playerController) =>  _playerController = playerController; 

        private void ChasePlayer()
        {
            var Xpos = _playerController.transform.position.x;
            var Ypos = _playerController.transform.position.y;
            
            _animator.SetFloat("VelocityX", Xpos);
            _animator.SetFloat("VelocityY", Ypos);
            
            
            gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, _playerController.transform.position, _currentSpeed * Time.deltaTime);
        }

        private void UpdateEnemyStats()
        {
            _currentHealth = _enemyConfig.EnemyHealth;
            _currentSpeed = _enemyConfig.EnemyMoveSpeed;
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        async void HitEffect()
        {
            if (_hitMaterial != null)
            {
                _hitEffectCts?.Cancel();
                _hitEffectCts = new CancellationTokenSource();

                try
                {
                    _spriteRenderer.material = _hitMaterial;
                    await Task.Delay(100, _hitEffectCts.Token);
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
