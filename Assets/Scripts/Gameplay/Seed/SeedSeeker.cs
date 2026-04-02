using Controllers;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
namespace Gameplay.Seed
{
    public class SeedSeeker : MonoBehaviour
    {
        [Header("Seed Config")]
        [SerializeField] private AssetReferenceT<SeedConfig> _seedConfigReference;
        [SerializeField] private PlayerController _playerController;

        private SeedConfig _seedConfig;
        private CircleCollider2D _circleCollider2D;
        private Transform _playerTransform;

        private void Awake()
        {
            _circleCollider2D = GetComponent<CircleCollider2D>();

            if (_playerController == null)
            {
                _playerController = GetComponentInParent<PlayerController>();
            }

            if (_playerController != null)
            {
                _playerTransform = _playerController.transform;
            }
        }

        private void Start()
        {
            InitializeSeedSeeker().Forget();
        }

        public Transform GetPlayerLocation()
        {
            return _playerTransform;
        }

        private async UniTask InitializeSeedSeeker()
        {
            _seedConfig = await _seedConfigReference.LoadAssetAsync<SeedConfig>().ToUniTask();

            if (_seedConfig == null || _circleCollider2D == null)
            {
                return;
            }

            _circleCollider2D.radius = _seedConfig.SeedSeekerRadius;
        }
    }
}
