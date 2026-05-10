using System;
using System.Collections.Generic;
using System.Threading;
using Collection;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Data.Upgrades;
using Managers;
using Obvious.Soap;
using PrimeTween;
using Shared.Enums;
using Shared.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class UpgradesController : MonoBehaviour
    {
        [Header("Seed Collection")]
        [SerializeField] private IntVariable _seedCollected;

        [Header("Animation Settings")] 
        [SerializeField] private float _animationDuration = 0.5f;

        [Header("Upgrades Settings")] 
        [SerializeField] private List<UpgradeData> _upgradesList;
        [SerializeField] private int _maxButtons;
        [SerializeField] private Transform _transform;
        
        private UpgradesManager _upgradesManager;
        private CanvasGroup _canvasGroup;
        private bool _canChoose;
        private int _randomIndex;
        private Button _button;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
        }

        private void Start()
        {
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();

            _canChoose = true;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable  = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            Events_Upgrades.OnActivateUpgradePanel += TurnOnCanvasGroup;
            Events_Upgrades.OnRoundStarted += TurnOffCanvasGroup;
            Events_Game.OnGameRestarted += OnGameRestarted;
            Events_Game.OnGameExited += OnGameRestarted;
        }

        private void OnDisable()
        {
            Events_Upgrades.OnActivateUpgradePanel -= TurnOnCanvasGroup;
            Events_Upgrades.OnRoundStarted -= TurnOffCanvasGroup;
            Events_Game.OnGameRestarted -= OnGameRestarted;
            Events_Game.OnGameExited -= OnGameRestarted;
        }

        private void OnGameRestarted()
        {
            if (_seedCollected != null)
            {
                _canChoose = true;
                _seedCollected.Value = 0;
                TurnOffCanvasGroup();
            }
        }

        private void ClearUpgrades()
        {
            if (_transform == null) return;
            foreach (Transform child in _transform)
            {
                Destroy(child.gameObject);
            }
        }

        private async UniTask SpawnUpgrades()
        {
            if (_upgradesList == null || _upgradesList.Count == 0) return;

            ClearUpgrades();

            // Create a temporary list of available (non-maxed) upgrades
            List<UpgradeData> availableUpgrades = new List<UpgradeData>();
            foreach (var upgrade in _upgradesList)
            {
                if (!upgrade.GetUpgradeLevelMaxed())
                {
                    availableUpgrades.Add(upgrade);
                }
            }

            // Determine how many buttons to spawn (minimum of _maxButtons or available count)
            int buttonsToSpawn = Mathf.Min(_maxButtons, availableUpgrades.Count);

            for (int i = 0; i < buttonsToSpawn; i++)
            {
                _randomIndex = Random.Range(0, availableUpgrades.Count);
                var upgrade = availableUpgrades[_randomIndex];

                // Remove from available list so it can't be picked again in this session
                availableUpgrades.RemoveAt(_randomIndex);

                _button = Instantiate(upgrade.ButtonPrefab, _transform);

                switch (upgrade.Category)
                {
                    case UpgradesCategoryType.Damage:
                        _button.onClick.AddListener(BuyDamageUpgrade);
                        break;
                    case UpgradesCategoryType.Range:
                        _button.onClick.AddListener(BuyRangeUpgrade);
                        break;
                    case UpgradesCategoryType.Speed:
                        _button.onClick.AddListener(BuySpeedUpgrade);
                        break;
                    case UpgradesCategoryType.Tomahawk:
                        _button.onClick.AddListener(BuyTomahawkUpgrade);
                        break;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }
        
        public async void TurnOffCanvasGroup()
        {
            if (_canvasGroup == null) return;
            
            await Tween.Alpha(_canvasGroup, 0f, _animationDuration).ToUniTask(this);
            
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                ClearUpgrades();
            }
        }
        
        public async void TurnOnCanvasGroup()
        {
            if (_canvasGroup == null) return;
            
            _canChoose = true;
            _canvasGroup.interactable  = true;
            _canvasGroup.blocksRaycasts = true;
            await Tween.Alpha(_canvasGroup, 1f, _animationDuration).ToUniTask(this);
            await SpawnUpgrades();
        }
        
        public void BuyDamageUpgrade()
        {
            if (!_canChoose) return;
            if (_upgradesManager == null) return;

            int newSeed = _upgradesManager.UpgradeDamage(_seedCollected.Value);
    
            if (newSeed != _seedCollected.Value) 
            {
                _seedCollected.Value = newSeed;
                _canChoose = false;
                TurnOffCanvasGroup();
            }
        }

        public void BuyRangeUpgrade()
        {
            if (!_canChoose) return;
            if (_upgradesManager == null) return;

            int newSeed = _upgradesManager.UpgradeRange(_seedCollected.Value);
    
            if (newSeed != _seedCollected.Value) 
            {
                _seedCollected.Value = newSeed;
                _canChoose = false;
                TurnOffCanvasGroup();
                
            }
        }

        public void BuySpeedUpgrade()
        {
            if (!_canChoose) return;
            if (_upgradesManager == null) return;

            int newSeed = _upgradesManager.UpgradeSpeed(_seedCollected.Value);
    
            if (newSeed != _seedCollected.Value) // ✅ only close if purchase succeeded
            {
                _seedCollected.Value = newSeed;
                _canChoose = false;
                TurnOffCanvasGroup();
            }
        }

        public void BuyTomahawkUpgrade()
        {
            if (!_canChoose) return;
            if (_upgradesManager == null) return;

            int newSeed = _upgradesManager.UpgradeTomahawk(_seedCollected.Value);
    
            if (newSeed != _seedCollected.Value) // ✅ only close if purchase succeeded
            {
                _seedCollected.Value = newSeed;
                _canChoose = false;
                TurnOffCanvasGroup();
            }
        }

       
    }
    
}
