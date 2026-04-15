using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace Gameplay.ButtonAnimator
{
    [RequireComponent(typeof(RectTransform))]
    public class ScaleUpDown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float _scaleFactor = 1.5f;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private RectTransform _rectTransform;
        private Tween _currentTween;

        private float _initialScaleX;
        private float _initialScaleY;
        private float _initialScaleZ;
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initialScaleX = _rectTransform.localScale.x;
            _initialScaleY = _rectTransform.localScale.y;
            _initialScaleZ = _rectTransform.localScale.z;
            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Mathf.Approximately(_rectTransform.localScale.x, _scaleFactor))
            {
                return;
            }
            _currentTween.Stop();
            _currentTween = Tween.Scale(_rectTransform, _scaleFactor, _duration, _ease);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Mathf.Approximately(_rectTransform.localScale.x, _initialScaleX))
            {
                return;
            }
            _currentTween.Stop();
            _currentTween = Tween.Scale(_rectTransform, _initialScaleX, _duration, _ease);
        }

        private void OnDestroy()
        {
            _currentTween.Stop();
        }
    }
}
