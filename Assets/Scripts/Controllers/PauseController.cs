using UnityEngine;

namespace Controllers
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        
        
        
        public void TurnOffCanvasGroup()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
        
        
        public void TurnOnCanvasGroup()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }
    }

}
