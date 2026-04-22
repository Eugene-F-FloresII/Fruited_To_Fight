using Obvious.Soap;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Shared.Events;


namespace Controllers
{
    public class EnemiesDefeatedController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _enemiesDefeatedText;
        
        private Transform _enemiesDefeatedGameObject;
        private int _enemiesDefeated;

        private void Start()
        {
            InitializeEnemyDefeated();
            UpdateUI();
        }

        private void OnEnable()
        {
            Events_Enemy.OnEnemyDeath += OnEnemyDefeated;
            Events_Game.OnGameRestarted += OnGameRestarted;
            Events_Game.OnGameExited += OnGameRestarted;
        }

        private void OnDisable()
        {
            Events_Enemy.OnEnemyDeath -= OnEnemyDefeated;
            Events_Game.OnGameRestarted -= OnGameRestarted;
            Events_Game.OnGameExited -= OnGameRestarted;
        }

        private void OnGameRestarted()
        {
            _enemiesDefeated = 0;
            UpdateUI();
        }

        private void OnEnemyDefeated()
        {
            _enemiesDefeated++;
            UpdateUI();
            IncreaseScale().Forget();
        }

        private void UpdateUI()
        {
            if (_enemiesDefeatedText != null)
            {
                _enemiesDefeatedText.text = "ENEMIES DEFEATED: " + _enemiesDefeated;
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
