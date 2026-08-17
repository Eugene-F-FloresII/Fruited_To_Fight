using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;


namespace Gameplay.UI
{
    public class IconAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _image;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private bool _isImage = true;

        [Header("Affliction Sprites")]
        [SerializeField] private List<Sprite> _afflictionSprites;
        [SerializeField] private float _fps = 12f;

        private int _animationVersion;

        private void OnEnable()
        {
            if (_afflictionSprites != null && _afflictionSprites.Count > 0)
            {
                _animationVersion++;

                if (_isImage)
                {
                  PlayImageAnimation(_afflictionSprites, _animationVersion).Forget();
                }
                else
                {
                    PlaySpriteRendererAnimation(_afflictionSprites, _animationVersion).Forget();
                }
            }
        }

        private void OnDisable()
        {
            _animationVersion++;
        }

        private async UniTaskVoid PlayImageAnimation(List<Sprite> sprites, int version)
        {
            int frame = 0;
            
            while (version == _animationVersion && this != null && _image != null)
            {
                _image.sprite = sprites[frame];

                frame = (frame + 1) % sprites.Count;

                int delay = Mathf.RoundToInt(1000f / _fps);
                await UniTask.Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }
        
        private async UniTaskVoid PlaySpriteRendererAnimation(List<Sprite> sprites, int version)
        {
            int frame = 0;
            
            while (version == _animationVersion && this != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = sprites[frame];

                frame = (frame + 1) % sprites.Count;

                int delay = Mathf.RoundToInt(1000f / _fps);
                await UniTask.Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }
    }

}
