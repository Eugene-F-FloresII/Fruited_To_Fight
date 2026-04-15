using Shared.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Controllers
{
    public class PauseController : MonoBehaviour
    {
        [Header("Control")]
        [SerializeField] private InputActionReference _inputActionReference;
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isPaused;

        private void OnEnable()
        {
            if (_inputActionReference != null)
            {
                _inputActionReference.action.Enable();
                _inputActionReference.action.performed += OnPausePerformed;
            }
            
            // Ensure we start in the correct state
            _isPaused = false;
            TurnOffCanvasGroup();
        }

        private void OnDisable()
        {
            if (_inputActionReference != null)
            {
                _inputActionReference.action.performed -= OnPausePerformed;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        public void TogglePause()
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            TurnOnCanvasGroup();
            Events_Game.OnGamePaused?.Invoke();
        }

        public void ResumeGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            TurnOffCanvasGroup();
            Events_Game.OnGameResumed?.Invoke();
        }

        public void RestartGame()
        {
            Debug.Log("Restarting Game...");
            Time.timeScale = 1f; // Always reset time scale before loading scene
            
            // Dispatch event so managers can reset ScriptableObject data
            Events_Game.OnGameRestarted?.Invoke();
            
            TurnOffCanvasGroup();
            
            // Reload the base scene (Gameplay) which is at index 0 in Build Settings.
            // This ensures we restart from the beginning and all additive scenes are reloaded correctly.
            SceneManager.LoadScene("Gameplay");
        }

        public void ExitGame()
        {
            Events_Game.OnGameExited?.Invoke();
            
            Time.timeScale = 1f; // Always reset time scale before loading scene
            SceneManager.LoadScene("MainMenu");
        }

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
