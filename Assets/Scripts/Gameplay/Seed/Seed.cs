using Collection;
using Controllers;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Shared.Events;

namespace Gameplay.Seed
{
    public class Seed : MonoBehaviour
    {
        [Header("Seed Config")]
        [SerializeField] private AssetReferenceT<SeedConfig> _seedConfigReference;

        private PlayerController _playerController;
        private SeedConfig _seedConfig;
        private Rigidbody2D _seedRb;
        private float _seedSpeed;
        private float _seedSeekerRadius;
        private SeedSeeker _seedSeeker;
        private bool _isInitialized;
        private bool _isInitializing;
        private bool _isTrackingPlayer = false;

        private void Awake()
        {
            _seedRb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            EnsureInitialized().Forget();
        }

        private void OnEnable()
        {
            EnsureInitialized().Forget();
            
            Events_Seed.OnEnemiesDefeated += EnemiesDefeated;
        }

        private void OnDisable()
        {
            if (_seedRb != null)
            {
                _seedRb.linearVelocity = Vector2.zero;
            }

            _isTrackingPlayer = false;
            
            Events_Seed.OnEnemiesDefeated -= EnemiesDefeated;
        }

        private void OnDestroy()
        {
            if (_seedConfigReference.IsValid())
            {
                _seedConfigReference.ReleaseAsset();
            }

            _isTrackingPlayer = false;
        }

        private void FixedUpdate()
        {
            if (_isTrackingPlayer)
            {
                TrackPlayer();
            }
            else
            {
                FollowPlayer();
            }
        }
        
        private void FollowPlayer()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (_seedSeeker == null)
            {
                return;
            }

            Transform playerTransform = _seedSeeker.GetPlayerLocation();

            if (playerTransform == null)
            {
                return;
            }

            Vector2 currentPosition = _seedRb != null ? _seedRb.position : (Vector2)transform.position;
            Vector2 targetPosition = playerTransform.position;
            float sqrDistance = (targetPosition - currentPosition).sqrMagnitude;
            float sqrRadius = _seedSeekerRadius * _seedSeekerRadius;

            if (sqrDistance > sqrRadius)
            {
                return;
            }

            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, _seedSpeed * Time.fixedDeltaTime);

            if (_seedRb != null)
            {
                _seedRb.MovePosition(nextPosition);
                return;
            }

            transform.position = nextPosition;
        }

        private void UpdateSeedStats()
        {
            _seedSpeed = _seedConfig.SeedSpeed;
            _seedSeekerRadius = _seedConfig.SeedSeekerRadius;
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
                _seedConfig = await _seedConfigReference.LoadAssetAsync<SeedConfig>().ToUniTask();

                if (_seedConfig == null)
                {
                    Debug.LogWarning($"{nameof(Seed)} failed to load SeedConfig.", this);
                    return;
                }

                UpdateSeedStats();
                _isInitialized = true;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void EnemiesDefeated()
        {
            if (_playerController != null)
            {
                _isTrackingPlayer = true;
            }
            
        }

        private void TrackPlayer()
        {
            if (!_isInitialized || _playerController == null)
            {
                return;
            }

            Vector2 currentPosition = _seedRb != null ? _seedRb.position : (Vector2)transform.position;
            Vector2 targetPosition = _playerController.transform.position;

            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, _seedSpeed * Time.fixedDeltaTime);

            if (_seedRb != null)
            {
                _seedRb.MovePosition(nextPosition);
            }
            else
            {
                transform.position = nextPosition;
            }
        }

        public void Initialize(SeedSeeker seedSeeker)
        {
            _seedSeeker = seedSeeker;
        }
    }

}
