using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Shared.Enums;
using UnityEngine;

namespace Gameplay.Weapons
{
    [Serializable]
    public struct AfflictionSpriteMapping
    {
        public AfflictionType AfflictionType;
        public List<Sprite> Sprites;
    }

    public class WeaponAfflictionSprite : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Affliction Sprites")]
        [SerializeField] private List<AfflictionSpriteMapping> _afflictionSprites;
        [SerializeField] private float _fps = 12f;

        private int _animationVersion;

        public void UpdateSprite(AfflictionType type)
        {
            if (_spriteRenderer == null)
            {
                Debug.LogWarning($"[WeaponAfflictionSprite] SpriteRenderer is null on {gameObject.name}", this);
                return;
            }

            foreach (var mapping in _afflictionSprites)
            {
                if (mapping.AfflictionType == type)
                {
                    if (mapping.Sprites == null || mapping.Sprites.Count == 0)
                    {
                        Debug.LogWarning($"[WeaponAfflictionSprite] No sprites assigned for {type}", this);
                        return;
                    }

                    _animationVersion++;
                    PlayAnimation(mapping.Sprites, _animationVersion).Forget();

                    return;
                }
            }

            Debug.LogWarning($"[WeaponAfflictionSprite] No sprite mapping found for AfflictionType: {type} on {gameObject.name}", this);
        }

        private async UniTaskVoid PlayAnimation(List<Sprite> sprites, int version)
        {
            int frame = 0;
            int delay = Mathf.RoundToInt(1000f / _fps);

            while (version == _animationVersion && this != null)
            {
                _spriteRenderer.sprite = sprites[frame];

                frame = (frame + 1) % sprites.Count;

                await UniTask.Delay(delay);
            }
        }
    }
}