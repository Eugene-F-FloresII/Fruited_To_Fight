using System;
using System.Collections.Generic;
using Collection;
using UnityEngine;

namespace Managers
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _userInterfaceGameObjects;
        [SerializeField] private CanvasGroup _gameUI;
        private void Awake()
        {
            ServiceLocator.Register(this);
            
            if (_userInterfaceGameObjects == null)
            {
                Debug.LogError("No GameObjects in " + gameObject.name);
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameUIManager>();
        }

        private void OnEnable()
        {
            Shared.Events.Events_Game.OnGameExited += TurnOffCanvasGroup;
            Shared.Events.Events_Game.OnGameRestarted += TurnOffCanvasGroup;
        }

        private void OnDisable()
        {
            Shared.Events.Events_Game.OnGameExited -= TurnOffCanvasGroup;
            Shared.Events.Events_Game.OnGameRestarted -= TurnOffCanvasGroup;
        }

        public void TurnOffCanvasGroup()
        {
            _gameUI.alpha = 0f;
            _gameUI.blocksRaycasts = false;
            _gameUI.interactable = false;
        }
        
        public void TurnOnCanvasGroup()
        {
            _gameUI.alpha = 1f;
            _gameUI.blocksRaycasts = true;
            _gameUI.interactable = true;
        }
    }

}
