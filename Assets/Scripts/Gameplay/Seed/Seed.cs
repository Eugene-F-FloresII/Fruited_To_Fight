using System;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Gameplay.Seed
{
    public class Seed : MonoBehaviour
    {
        [Header("Seed Config")]
        [SerializeField] private AssetReferenceT<SeedConfig> _seedConfigReference;

        private SeedConfig _seedConfig;
        private float _seedSpeed ;
        private Transform _playerTransform;
        private bool _playerLocDetected;

        private void Start()
        {
            InitializeSeed().Forget();
        }

        private void OnEnable()
        {
            UpdateSeedStats();
        }

        private void OnDisable()
        {
            
        }

        private void FixedUpdate()
        {
            FollowPlayer();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out SeedSeeker seedSeeker))
            {
                _playerTransform = seedSeeker.GetPlayerLocation();
                _playerLocDetected = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out SeedSeeker seedSeeker))
            {
                _playerTransform = seedSeeker.GetPlayerLocation();
                _playerLocDetected = false;
            }
        }
        
        private void FollowPlayer()
        {
            if (_playerTransform != null && _playerLocDetected)
            {
                transform.position = Vector2.MoveTowards(transform.position, _playerTransform.position, _seedSpeed);
            }
        }

        private void UpdateSeedStats()
        {
            _seedSpeed = _seedConfig.SeedSpeed;
        }

        private async UniTask InitializeSeed()
        {
            _seedConfig = await _seedConfigReference.LoadAssetAsync<SeedConfig>().ToUniTask();
            
            UpdateSeedStats();
        }
    }

}
