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

        public void TurnOffGameUI()
        {
            _gameUI.alpha = 0f;
            _gameUI.blocksRaycasts = false;
            _gameUI.interactable = false;
        }
        
        public void TurnOnGameUI()
        {
            _gameUI.alpha = 1f;
            _gameUI.blocksRaycasts = true;
            _gameUI.interactable = true;
        }
    }

}
