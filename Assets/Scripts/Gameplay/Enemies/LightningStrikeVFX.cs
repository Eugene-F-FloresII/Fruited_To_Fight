using UnityEngine;

namespace Gameplay.Enemies
{
    /// <summary>
    /// Stretches the lightning bolt sprite to exactly reach the top of the screen when enabled.
    /// </summary>
    public class LightningStrikeVFX : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _triangleRenderer;
        
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
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
