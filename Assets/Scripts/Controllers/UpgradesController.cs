using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using Collection;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Managers;
using Obvious.Soap;
using PrimeTween;
using Shared.Enums;
using Shared.Events;

namespace Controllers
{
    public class UpgradesController : MonoBehaviour
    {
        [SerializeField] private IntVariable _seedCollected;

        [Header("Animation Settings")] 
        [SerializeField] private float _animationDuration = 0.5f;
        
        private UpgradesManager _upgradesManager;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
        }

        private void Start()
        {
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();
            
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable  = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            Events_Upgrades.OnActivateUpgradePanel += TurnOnCanvasGroup;
            Events_Upgrades.OnRoundStarted += TurnOffCanvasGroup;
        }

        private void OnDisable()
        {
            Events_Upgrades.OnActivateUpgradePanel -= TurnOnCanvasGroup;
            Events_Upgrades.OnRoundStarted -= TurnOffCanvasGroup;
        }

        public void BuyDamageUpgrade()
        {
            if (_upgradesManager == null || _upgradesManager.GetDamageLevelMaxed()) return;

            int newSeed = _upgradesManager.UpgradeOverallDamage(_seedCollected.Value);
    
            if (newSeed != _seedCollected.Value) 
            {
                _seedCollected.Value = newSeed;
                TurnOffCanvasGroup();
            }
        }

        public void BuyRangeUpgrade()
        {
            if (_upgradesManager == null || _upgradesManager.GetRangedLevelMaxed()) return;

            int newSeed = _upgradesManager.UpgradeOverallRange(_seedCollected.Value);
    
            if (newSeed != _seedCollected.Value) 
            {
                _seedCollected.Value = newSeed;
                TurnOffCanvasGroup();
            }
        }

        public void BuySpeedUpgrade()
        {
            if (_upgradesManager == null || _upgradesManager.GetSpeedLevelMaxed()) return;

            int newSeed = _upgradesManager.UpgradeOverallSpeed(_seedCollected.Value);
    
            if (newSeed != _seedCollected.Value) // ✅ only close if purchase succeeded
            {
                _seedCollected.Value = newSeed;
                TurnOffCanvasGroup();
            }
        }

        private async void TurnOffCanvasGroup()
        {
            await Tween.Alpha(_canvasGroup, 0f, _animationDuration).ToUniTask();
            _canvasGroup.interactable  = false;
            _canvasGroup.blocksRaycasts = false;
        }
        
        private async void TurnOnCanvasGroup()
        {
            _canvasGroup.interactable  = true;
            _canvasGroup.blocksRaycasts = true;
            await Tween.Alpha(_canvasGroup, 1f, _animationDuration).ToUniTask();
        }
    }
    
}
