using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// Handles opening the submit score confirmation panel by setting its CanvasGroup 
    /// properties and triggering its pop-up animation when Btn_SubmitScore is pressed.
    /// </summary>
    public class OpenSubmitScoreConfirmation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _btnSubmitScore;
        [SerializeField] private CanvasGroup _confirmationCanvasGroup;
        [SerializeField] private SubmitScoreConfirmationAnimation _confirmationAnimation;

        private void Awake()
        {
            if (_btnSubmitScore == null)
            {
                _btnSubmitScore = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (_btnSubmitScore != null)
            {
                _btnSubmitScore.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (_btnSubmitScore != null)
            {
                _btnSubmitScore.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            if (_confirmationCanvasGroup != null)
            {
                _confirmationCanvasGroup.alpha = 1f;
                _confirmationCanvasGroup.interactable = true;
                _confirmationCanvasGroup.blocksRaycasts = true;
            }

            if (_confirmationAnimation != null)
            {
                _confirmationAnimation.PlayPopUp();
            }
        }
    }
}
