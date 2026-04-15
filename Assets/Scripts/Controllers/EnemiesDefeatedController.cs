using System;
using Obvious.Soap;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PrimeTween;


namespace Controllers
{
    public class EnemiesDefeatedController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _enemiesDefeatedText;
        [SerializeField] private IntVariable _enemiesDefeatedCount;
        
        private Transform _enemiesDefeatedGameObject;

        private void Start()
        {
            InitializeEnemyDefeated();
        }

        private void OnEnable()
        {
            if (_enemiesDefeatedCount != null)
            {
                _enemiesDefeatedCount.OnValueChanged += OnEnemyDefeated;
            }
            Shared.Events.Events_Game.OnGameRestarted += OnGameRestarted;
        }

        private void OnDisable()
        {
            if (_enemiesDefeatedCount != null)
            {
                _enemiesDefeatedCount.OnValueChanged -= OnEnemyDefeated;
            }
            Shared.Events.Events_Game.OnGameRestarted -= OnGameRestarted;
        }

        private void OnGameRestarted()
        {
            if (_enemiesDefeatedCount != null)
            {
                _enemiesDefeatedCount.Value = 0;
                UpdateUI(0);
            }
        }

        private void OnEnemyDefeated(int i)
        {
            UpdateUI(i);
            IncreaseScale().Forget();
        }

        private void UpdateUI(int count)
        {
            if (_enemiesDefeatedText != null)
            {
                _enemiesDefeatedText.text = "ENEMIES DEFEATED: " + count;
            }
        }

        private void InitializeEnemyDefeated()
        {
            if (_enemiesDefeatedGameObject == null && _enemiesDefeatedText != null)
            { 
                _enemiesDefeatedGameObject = _enemiesDefeatedText.transform;
            }
        }
        
        private async UniTaskVoid IncreaseScale()
        {
            if (_enemiesDefeatedGameObject == null)
            {
                return;
            }

            bool canceled = await Tween.Scale(_enemiesDefeatedGameObject, Vector3.one * 1.3f, duration: 0.3f, Ease.OutBack)
                .ToUniTask(this)
                .SuppressCancellationThrow();

            if (canceled || _enemiesDefeatedGameObject == null) return;
            
            await Tween.Scale(_enemiesDefeatedGameObject, Vector3.one, duration: 0.3f, Ease.InBack)
                .ToUniTask(this)
                .SuppressCancellationThrow();
            
        }
    }

}
