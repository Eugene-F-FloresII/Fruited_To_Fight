using System;
using Shared.Enums;
using Shared.Events;
using TMPro;
using UnityEngine;

namespace Controllers
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _gameConditionText;
        private CanvasGroup _canvasGroup;


        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        private void Start()
        {
            TurnOffCanvasGroup();
        }

        private void OnEnable()
        {
            Events_Game.OnGameRestarted += TurnOffCanvasGroup;
            Events_Game.OnGameExited += TurnOffCanvasGroup;
            Events_Game.OnGameFinished += InitializeGameCondition;
        }

        private void OnDisable()
        {
            Events_Game.OnGameRestarted -= TurnOffCanvasGroup;
            Events_Game.OnGameExited -= TurnOffCanvasGroup;
            Events_Game.OnGameFinished -= InitializeGameCondition;
        }

        public void BackToMainMenu()
        {
            Events_Game.OnGameExited?.Invoke();
            Events_Game.OnSceneChange?.Invoke("MainMenu");
        }

        public void RestartGame()
        {
            Events_Game.OnGameRestarted?.Invoke();
            Events_Game.OnSceneChange?.Invoke("Gameplay");
        }
        
        private void TurnOffCanvasGroup()
        {
            Time.timeScale = 1f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
        }
        
        private void TurnOnCanvasGroup()
        {
            Time.timeScale = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        }

        private void InitializeGameCondition(GameCondition condition)
        {
            TurnOnCanvasGroup();
            
            if (condition == GameCondition.Win)
            {
                _gameConditionText.text = "YOU WIN";
                _gameConditionText.color = Color.green;
            }
            else
            {
                _gameConditionText.text = "YOU LOSE";
                _gameConditionText.color = Color.red;
            }
        }
    }

}
