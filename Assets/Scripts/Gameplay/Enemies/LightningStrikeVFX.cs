using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Shared.Events;
using UnityEngine;

namespace Gameplay.Enemies
{
    /// <summary>
    /// Stretches the lightning bolt sprite to exactly reach the top of the screen when enabled.
    /// </summary>
    public class LightningStrikeVFX : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _triangleRenderer;
        [SerializeField] private float _shakeForce = 1.0f;
        [SerializeField] private float _shakeDelay = 0.15f;

        [Header("Sound Settings")]
        [SerializeField] private AudioClip _strikeSFX;
        [Range(0f, 1f)]
        [SerializeField] private float _strikeSFXVolume = 1f;
        [SerializeField] private AudioClip _explosionSFX;
        [Range(0f, 1f)]
        [SerializeField] private float _explosionSFXVolume = 1f;

        private Camera _mainCamera;
        private CancellationTokenSource _shakeCts;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            PlayStrikeSFX();
            _shakeCts = new CancellationTokenSource();
            TriggerShakeAndExplosionSFXAsync().Forget();
        }

        private void OnDisable()
        {
            if (_shakeCts != null)
            {
                _shakeCts.Cancel();
                _shakeCts.Dispose();
                _shakeCts = null;
            }
        }

        /// <summary>
        /// Plays the lightning strike sound when the VFX is first enabled.
        /// </summary>
        private void PlayStrikeSFX()
        {
            if (_strikeSFX != null)
            {
                Events_Sound.PlaySoundWithVolume?.Invoke(_strikeSFX, _strikeSFXVolume);
            }
        }

        /// <summary>
        /// Delays the screen shake and explosion sound to match when the lightning hits the ground.
        /// </summary>
        private async UniTaskVoid TriggerShakeAndExplosionSFXAsync()
        {
            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_shakeDelay), cancellationToken: _shakeCts.Token).SuppressCancellationThrow();
            if (isCanceled) return;

            Shared.Events.Events_Character.RequestShake(_shakeForce);

            if (_explosionSFX != null)
            {
                Events_Sound.PlaySoundWithVolume?.Invoke(_explosionSFX, _explosionSFXVolume);
            }
        }

        private void LateUpdate()
        {
            StretchToTop();
        }

        /// <summary>
        /// Calculates the distance to the top of the camera viewport and stretches the Triangle sprite's Y-scale to fit.
        /// </summary>
        private void StretchToTop()
        {
            if (_triangleRenderer == null || _triangleRenderer.sprite == null || _mainCamera == null) return;

            // Get the Z distance to calculate correct world point
            float zDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
            
            // ViewportToWorldPoint: (0.5, 1) is top-center of the screen
            Vector3 topWorldPos = _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, zDistance));

            // Calculate the distance from this object (spawn point) to the top of the screen
            float distanceToTop = topWorldPos.y - transform.position.y;

            if (distanceToTop <= 0) return;

            // The default unscaled height of the sprite in Unity units
            float unscaledSpriteHeight = _triangleRenderer.sprite.bounds.size.y;
            
            if (unscaledSpriteHeight == 0) return;

            // Calculate required scale. We add a little extra (e.g. 1.0f) to ensure it bleeds past the top edge
            float requiredScale = (distanceToTop / unscaledSpriteHeight) + 1.0f;

            // Apply the new Y scale to the Triangle
            Transform triangleTransform = _triangleRenderer.transform;
            triangleTransform.localScale = new Vector3(
                triangleTransform.localScale.x,
                requiredScale,
                triangleTransform.localScale.z
            );
        }
    }
}
