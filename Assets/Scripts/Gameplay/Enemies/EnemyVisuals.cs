using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using Shared.Events;
using Data;

namespace Gameplay.Enemies
{
    public class EnemyVisuals : MonoBehaviour
    {
        [Header("Material References")]
        [SerializeField] private Material _hitMaterial;
        [SerializeField] private Material _defaultMaterial;

        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private EnemyStats _enemyStats;
        private CancellationTokenSource _hitEffectCts;
        private readonly string _velocityX = "VelocityX";
        private readonly string _velocityY = "VelocityY";

        private void Awake()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnDisable()
        {
            if (_hitEffectCts != null)
            {
                _hitEffectCts.Cancel();
                _hitEffectCts.Dispose();
                _hitEffectCts = null;
            }

            if (_spriteRenderer != null && _defaultMaterial != null)
                _spriteRenderer.material = _defaultMaterial;
        }

        /// <summary>
        /// Initializes the reference to EnemyStats for config-driven audio.
        /// </summary>
        public void Initialize(EnemyStats enemyStats)
        {
            _enemyStats = enemyStats;
        }

        public void PlayHitEffectAndSound()
        {
            if (_enemyStats != null && _enemyStats.Config != null && _enemyStats.Config.HitSFX != null)
            {
                Events_Sound.PlaySoundWithVolume?.Invoke(_enemyStats.Config.HitSFX, _enemyStats.Config.HitSFXVolume);
            }

            if (_hitEffectCts != null)
            {
                _hitEffectCts.Cancel();
                _hitEffectCts.Dispose();
            }
            _hitEffectCts = new CancellationTokenSource();
            HitEffectAsync(_hitEffectCts.Token).Forget();
        }

        private async UniTask HitEffectAsync(CancellationToken token)
        {
            if (_hitMaterial != null && _spriteRenderer != null)
            {
                try
                {
                    _spriteRenderer.material = _hitMaterial;
                    await UniTask.Delay(150, cancellationToken: token);
                    if (_spriteRenderer != null)
                        _spriteRenderer.material = _defaultMaterial;
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e) when (e is MissingReferenceException || e is ObjectDisposedException)
                {
                    Debug.Log("Entity Dead");
                }
            }
        }

        public void SetAnimationVelocity(float x, float y)
        {
            if (_animator != null)
            {
                _animator.SetFloat(_velocityX, x);
                _animator.SetFloat(_velocityY, y);
            }
        }

        public void SetAnimationSpeed(float speed)
        {
            if (_animator != null)
            {
                _animator.speed = speed;
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(Material hit, Material def, Animator anim, SpriteRenderer sr)
        {
            _hitMaterial = hit;
            _defaultMaterial = def;
            _animator = anim;
            _spriteRenderer = sr;
        }
#endif
    }
}
