using UnityEngine;
using PrimeTween;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Handles the visual warning circle indicator before a spike or meteor impact.
    /// It scales up and fades out over its defined lifetime, then self-destructs.
    /// </summary>
    public class AbilityWarningCircle : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Sequence _activeSequence;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void OnDisable()
        {
            _activeSequence.Stop();
        }

        /// <summary>
        /// Initializes the warning circle with a target radius and animation duration.
        /// </summary>
        /// <param name="radius">Target circular radius of the warning indicator.</param>
        /// <param name="duration">How long the warning circle remains active before disappearing.</param>
        public void Initialize(float radius, float duration)
        {
            _activeSequence.Stop();

            if (_spriteRenderer != null)
            {
                // Reset color opacity
                Color color = _spriteRenderer.color;
                color.a = 0.4f;
                _spriteRenderer.color = color;
            }

            // A typical 2D Sprite of radius 0.5 (diameter 1.0) needs to be scaled by radius * 2
            float targetDiameter = radius * 2f;
            transform.localScale = Vector3.zero;

            _activeSequence = Sequence.Create()
                .Group(Tween.Scale(transform, new Vector3(targetDiameter, targetDiameter, 1f), duration * 0.3f, Ease.OutQuad))
                .Chain(Tween.Delay(duration * 0.4f))
                .Chain(Tween.Scale(transform, Vector3.zero, duration * 0.3f, Ease.InQuad))
                .Group(Tween.Alpha(_spriteRenderer, 0f, duration * 0.3f, Ease.InQuad))
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
