using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
using Cysharp.Threading.Tasks;
using Controllers;
using Data;
using Shared.Enums;
using Shared.Events;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Animates a falling meteor using a Sprite Atlas. Upon reaching the impact frame,
    /// it instantiates a physical trigger collider object to deal damage in an area,
    /// then destroys itself once the animation completes.
    /// </summary>
    public class Meteor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        [Header("Sprite Atlas Animation")]
        [SerializeField] private SpriteAtlas _spriteAtlas;
        [SerializeField] private string _spritePrefix = "1_effect_fire_";
        [SerializeField] private int _frameCount = 19;
        [SerializeField] private float _fixedFps = 12f;
        [SerializeField] private bool _loopAnimation = false;

        [Header("Damage Settings")]
        [SerializeField] private GameObject _explosionPrefab;
        [SerializeField] private int _damageFrameIndex = 4;
        [SerializeField] private float _explosionRadius = 2.5f;

        private List<Sprite> _animationSpritesList = new List<Sprite>();
        private WeaponConfig _weaponConfig;
        private float _percentageIncrease;
        private CancellationTokenSource _animationCts;
        private int _animationVersion;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            CacheSpritesFromAtlas();
        }

        private void OnDisable()
        {
            _animationVersion++;
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;
        }

        /// <summary>
        /// Caches the animation sprites from the Sprite Atlas on startup to prevent allocation overhead during animation.
        /// </summary>
        private void CacheSpritesFromAtlas()
        {
            if (_spriteAtlas == null) return;

            _animationSpritesList.Clear();
            for (int i = 0; i < _frameCount; i++)
            {
                // Retrieve sprite by name from the atlas (format index to 3 digits e.g. 000, 001)
                Sprite sprite = _spriteAtlas.GetSprite(_spritePrefix + i.ToString("D3"));
                if (sprite != null)
                {
                    _animationSpritesList.Add(sprite);
                }
                else
                {
                    Debug.LogWarning($"[Meteor] Sprite '{_spritePrefix}{i.ToString("D3")}' not found in SpriteAtlas.");
                }
            }
        }

        /// <summary>
        /// Animates the sprite renderer sequentially using the cached sprites from the atlas.
        /// </summary>
        /// <param name="version">The current animation version to guard against overlapping loops.</param>
        private async UniTaskVoid PlayAnimationAsync(int version)
        {
            int frame = 0;
            float frameDelay = 1f / _fixedFps;

            while (version == _animationVersion && this != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _animationSpritesList[frame];
                
                // Instantiate the physical explosion trigger exactly on the impact frame
                if (frame == _damageFrameIndex)
                {
                    InstantiateExplosion();
                }

                if (_loopAnimation)
                {
                    frame = (frame + 1) % _animationSpritesList.Count;
                }
                else
                {
                    frame++;
                    if (frame >= _animationSpritesList.Count)
                    {
                        break;
                    }
                }

                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(frameDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (version != _animationVersion) return;
            }

            // Destroy the meteor after the animation has finished playing
            if (version == _animationVersion && !_loopAnimation && gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Instantiates the physical explosion trigger GameObject at the impact position.
        /// </summary>
        private void InstantiateExplosion()
        {
            if (_explosionPrefab == null || _weaponConfig == null) return;

            GameObject explosionGo = Instantiate(_explosionPrefab, transform.position, Quaternion.identity, null);
            if (explosionGo.TryGetComponent(out MeteorExplosion explosion))
            {
                explosion.Initialize(_weaponConfig, _percentageIncrease, _explosionRadius);
            }
        }

        /// <summary>
        /// Initializes the meteor with stats, damage multipliers, and target landing coordinates.
        /// </summary>
        /// <param name="config">The weapon configuration for damage/knockback values.</param>
        /// <param name="percentage">The ability damage multiplier.</param>
        /// <param name="landingPosition">The target landing position.</param>
        public void Initialize(WeaponConfig config, float percentage, Vector3 landingPosition)
        {
            // Detach the root GameObject from any hierarchy to prevent following
            transform.SetParent(null);

            transform.position = landingPosition;
            _weaponConfig = config;
            _percentageIncrease = percentage;

            _animationVersion++;
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = new CancellationTokenSource();

            PlayAnimationAsync(_animationVersion).Forget();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
