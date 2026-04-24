using System;
using Shared.Events;
using UnityEngine;

namespace Controllers
{
    public class ObjectivesController : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            TurnOnCanvasGroup();
        }

        private void OnEnable()
        {
            Events_Game.OnGameExited += TurnOffCanvasGroup;
        }

        private void OnDisable()
        {
            Events_Game.OnGameExited -= TurnOffCanvasGroup;
        }

        private void TurnOffCanvasGroup()
        {
            _canvasGroup.alpha = 0f;
        }

        private void TurnOnCanvasGroup()
        {
            _canvasGroup.alpha = 1f;
        }
    }

}
