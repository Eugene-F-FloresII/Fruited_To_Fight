using UnityEngine;
using PrimeTween;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Handles the programmatic movement animation of the melee spear thrust using PrimeTween.
    /// Moves the local position of the visual forward in the thrust direction, then retreats it
    /// and deactivates the GameObject to return it to the object pool.
    /// </summary>
    public class SpearThrustVisual : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _defaultDuration = 0.2f;

        private Vector3 _originalLocalPos;
        private Sequence _activeSequence;

        private void Awake()
        {
            _originalLocalPos = transform.localPosition;
        }

        private void OnDisable()
        {
            _activeSequence.Stop();
            // Reset position to original on disable
            transform.localPosition = _originalLocalPos;
        }

        /// <summary>
        /// Triggers the thrust animation.
        /// </summary>
        /// <param name="direction">The direction vector of the stab.</param>
        /// <param name="range">The weapon range, used to scale the thrust distance.</param>
        /// <param name="attackSpeed">The attack speed cooldown, used to cap duration.</param>
        public void AnimateThrust(Vector2 direction, float range, float attackSpeed)
        {
            _activeSequence.Stop();

            // Set initial position
            transform.localPosition = _originalLocalPos;

            // Calculate duration to fit within attack speed window
            float duration = Mathf.Min(_defaultDuration, attackSpeed * 0.5f);
            
            // Thrust forward distance proportional to range (e.g., 40% of range)
            float thrustDistance = range * 0.4f;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + (Vector3)direction.normalized * thrustDistance;

            _activeSequence = Sequence.Create()
                .Group(Tween.Position(transform, targetPosition, duration * 0.4f, Ease.OutQuad))
                .Chain(Tween.Position(transform, startPosition, duration * 0.6f, Ease.InQuad))
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
