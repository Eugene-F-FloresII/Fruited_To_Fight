using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace UI
{
    public class FloatingJoystick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath;

        [SerializeField] private Canvas _parentCanvas;
        [SerializeField] private RectTransform _joystickBackground;
        [SerializeField] private RectTransform _joystickKnob;
        [SerializeField] private float _movementRange = 150f;

        private Vector2 _anchorPosition;

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        private void Start()
        {
            HideJoystick();
        }

        private void HideJoystick()
        {
            if (_joystickBackground != null)
            {
                _joystickBackground.gameObject.SetActive(false);
            }
            
            if (_joystickKnob != null)
            {
                _joystickKnob.localPosition = Vector2.zero;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_joystickBackground == null || _parentCanvas == null) return;

            _joystickBackground.gameObject.SetActive(true);

            Camera cam = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform, 
                eventData.position, 
                cam, 
                out Vector2 localPoint);

            _joystickBackground.localPosition = localPoint;
            _anchorPosition = localPoint;
            
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_joystickBackground == null || _parentCanvas == null || _joystickKnob == null) return;

            Camera cam = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform, 
                eventData.position, 
                cam, 
                out Vector2 localPoint);

            Vector2 direction = localPoint - _anchorPosition;
            float distance = direction.magnitude;
            
            Vector2 normalizedDirection = direction.normalized;
            Vector2 knobPosition = normalizedDirection * Mathf.Clamp(distance, 0f, _movementRange);
            
            _joystickKnob.localPosition = knobPosition;

            // Send normalized value to the Unity Input System
            Vector2 inputVector = knobPosition / _movementRange;
            SendValueToControl(inputVector);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HideJoystick();
            SendValueToControl(Vector2.zero);
        }
    }
}
