using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Data;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Animates a sequence of sprites for a weapon slash effect.
    /// Logic is similar to IconAnimator but uses SpriteRenderer and adapts to WeaponAtkSpeed.
    /// </summary>
    public class SlashAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private WeaponConfig _weaponConfig;

        [Header("Animation Settings")]
        [SerializeField] private List<Sprite> _slashSprites;
        
        [Tooltip("If true, the animation duration will match the weapon's attack speed. If false, it uses a fixed FPS.")]
        [SerializeField] private bool _useWeaponAttackSpeed = true;
        [SerializeField] private float _fixedFps = 12f;

        private int _animationVersion;

        private void OnEnable()
        {
            if (_slashSprites != null && _slashSprites.Count > 0)
            {
                _animationVersion++;
                PlayAnimation(_animationVersion).Forget();
            }
        }

        private void OnDisable()
        {
            _animationVersion++;
        }

        private async UniTaskVoid PlayAnimation(int version)
        {
            int frame = 0;
            float frameDelaySeconds;

            if (_useWeaponAttackSpeed && _weaponConfig != null)
            {
                // Divide total attack speed duration by number of frames to fit exactly
                frameDelaySeconds = _weaponConfig.WeaponAtkSpeed / _slashSprites.Count;
            }
            else
            {
                frameDelaySeconds = 1f / _fixedFps;
            }

            while (version == _animationVersion && this != null && _spriteRenderer != null && frame < _slashSprites.Count)
            {
                _spriteRenderer.sprite = _slashSprites[frame];
                frame++;

                try 
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(frameDelaySeconds), cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                // If version changed during delay, exit
                if (version != _animationVersion) return;
            }

            // Deactivate object after animation finishes to return to pool
            if (version == _animationVersion && gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
