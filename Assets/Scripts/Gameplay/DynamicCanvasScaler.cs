using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    [RequireComponent(typeof(CanvasScaler))]
    public class DynamicCanvasScaler : MonoBehaviour
    {
        private CanvasScaler _scaler;

        // Your reference aspect (1080/1920)
        private const float ReferenceAspect = 9f / 16f;

        void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
            AdjustScale();
        }

        void AdjustScale()
        {
            float currentAspect = (float)Screen.width / Screen.height;

            // If current screen is wider than reference (e.g. tablet)
            // blend toward matching width so content doesn't get too big
            if (currentAspect > ReferenceAspect)
            {
                _scaler.matchWidthOrHeight = 0f; // match width
            }
            else
            {
                _scaler.matchWidthOrHeight = 1f; // match height
            }
        }
    }

}
