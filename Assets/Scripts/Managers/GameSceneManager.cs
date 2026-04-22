using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers 
{
    public class GameSceneManager : MonoBehaviour
    {
        private bool _addedUI;
        void Start()
        {
            AddUIScene();
        }

        private void OnDisable()
        {
            _addedUI = false;
        }

        private void OnDestroy()
        {
            _addedUI = false;
        }

        private void AddUIScene()
        {
            while (!_addedUI)
            {
                SceneManager.LoadScene("UI", LoadSceneMode.Additive);
                _addedUI = true;
            }
        }
    
    }
}