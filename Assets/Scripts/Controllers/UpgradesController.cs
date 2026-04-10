using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using PrimeTween;
using Shared.Enums;
using Shared.Events;

namespace Controllers
{
    public class UpgradesController : MonoBehaviour
    {
        [Header("Weapons to Upgrade **Do not touch**")]
        [SerializeField] private WeaponConfig _firstWeaponConfig;
        [SerializeField] private WeaponConfig _secondWeaponConfig;
        
        [Header("Currency")]
        [SerializeField] private IntVariable _seeds;
        
        [Header("Upgrade Levels")]
        [SerializeField] private IntVariable _firstDamageLevels;
        [SerializeField] private IntVariable _firstRangeLevels;
        [SerializeField] private IntVariable _firstAtkSpeedLevels;
        [SerializeField] private IntVariable _secondDamageLevels;
        [SerializeField] private IntVariable _secondRangeLevels;
        [SerializeField] private IntVariable _secondAtkSpeedLevels;

        [Header("Upgrade Panel")]
        [SerializeField] private CanvasGroup _upgradesCanvasGroup;
        [SerializeField] private RectTransform _upgradesPanelRectTransform;

        [Header("Active Upgrade Weapon")]
        [SerializeField] private UpgradesWeaponSlot _activeUpgradeWeaponSlot = UpgradesWeaponSlot.First;

        private bool _configureFirstWeapon;
        private bool _isFirstOccupied = false;
        private bool _isSecondOccupied = false;

        private float _firstWeaponInitialDamage;
        private int _firstWeaponInitialPierce;
        private float _firstWeaponInitialRange;
        private float _firstWeaponInitialKnockback;
        private float _firstWeaponInitialSpeed;
        private float _firstWeaponInitialAtkSpeed;
        
        private float _secondWeaponInitialDamage;
        private int _secondWeaponInitialPierce;
        private float _secondWeaponInitialRange;
        private float _secondWeaponInitialKnockback;
        private float _secondWeaponInitialSpeed;
        private float _secondWeaponInitialAtkSpeed;


        private float _firstDamagePercentage = 0.25f;
        private float _firstRangePercentage = 0.25f;
        private float _firstSpeedPercentage = 0.25f;
        
        private float _secondDamagePercentage = 0.25f;
        private float _secondRangePercentage = 0.25f;
        private float _secondSpeedPercentage = 0.25f;

        private Sequence _showPanelSequence;
        private Tween _closePanelTween;
        private CancellationTokenSource _panelCloseCts;
         
        private int _firstPriceDamageUpgrade = 10;
        private int _firstPriceRangeUpgrade = 7;
        private int _firstPriceSpeedUpgrade = 9;
        
        private int _secondPriceDamageUpgrade = 10;
        private int _secondPriceRangeUpgrade = 7;
        private int _secondPriceSpeedUpgrade = 9;
        
        private int _maxLevel = 4;
        public int MaxLevel => _maxLevel;

        private void Awake()
        {
            if (_upgradesCanvasGroup == null)
            {
                _upgradesCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (_upgradesPanelRectTransform == null)
            {
                _upgradesPanelRectTransform = GetComponent<RectTransform>();
            }
        }
        
        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += CurrentWeapon;
            Events_Round.OnRoundEnded += ShowUpgradePanel;
            Events_Round.OnRoundStarted += HideUpgradePanel;

            if (_seeds != null)
            {
                _seeds.OnValueChanged += HandleSeedsChanged;
                SetSeedsClamped(_seeds.Value);
            }

            SubscribeLevelEvents();
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= CurrentWeapon;
            Events_Round.OnRoundEnded -= ShowUpgradePanel;
            Events_Round.OnRoundStarted -= HideUpgradePanel;

            if (_seeds != null)
            {
                _seeds.OnValueChanged -= HandleSeedsChanged;
            }

            UnsubscribeLevelEvents();

            if (_showPanelSequence.isAlive)
            {
                _showPanelSequence.Stop();
            }

            if (_closePanelTween.isAlive)
            {
                _closePanelTween.Stop();
            }

            CancelPanelClose();
        }

        private void HandleSeedsChanged(int seeds)
        {
            if (seeds < 0)
            {
                SetSeedsClamped(0);
            }
        }

        private void RaiseUpgradeDataChanged(UpgradesWeaponSlot slot, UpgradesPanelType upgradesPanelType)
        {
            Events_Upgrades.OnUpgradeDataChanged?.Invoke(slot, upgradesPanelType);
        }

        private void SubscribeLevelEvents()
        {
            if (_firstDamageLevels != null) _firstDamageLevels.OnValueChanged += HandleFirstDamageLevelChanged;
            if (_firstRangeLevels != null) _firstRangeLevels.OnValueChanged += HandleFirstRangeLevelChanged;
            if (_firstAtkSpeedLevels != null) _firstAtkSpeedLevels.OnValueChanged += HandleFirstSpeedLevelChanged;
            if (_secondDamageLevels != null) _secondDamageLevels.OnValueChanged += HandleSecondDamageLevelChanged;
            if (_secondRangeLevels != null) _secondRangeLevels.OnValueChanged += HandleSecondRangeLevelChanged;
            if (_secondAtkSpeedLevels != null) _secondAtkSpeedLevels.OnValueChanged += HandleSecondSpeedLevelChanged;
        }

        private void UnsubscribeLevelEvents()
        {
            if (_firstDamageLevels != null) _firstDamageLevels.OnValueChanged -= HandleFirstDamageLevelChanged;
            if (_firstRangeLevels != null) _firstRangeLevels.OnValueChanged -= HandleFirstRangeLevelChanged;
            if (_firstAtkSpeedLevels != null) _firstAtkSpeedLevels.OnValueChanged -= HandleFirstSpeedLevelChanged;
            if (_secondDamageLevels != null) _secondDamageLevels.OnValueChanged -= HandleSecondDamageLevelChanged;
            if (_secondRangeLevels != null) _secondRangeLevels.OnValueChanged -= HandleSecondRangeLevelChanged;
            if (_secondAtkSpeedLevels != null) _secondAtkSpeedLevels.OnValueChanged -= HandleSecondSpeedLevelChanged;
        }

        private void HandleFirstDamageLevelChanged(int _)
        {
            RaiseUpgradeDataChanged(UpgradesWeaponSlot.First, UpgradesPanelType.Damage);
        }

        private void HandleFirstRangeLevelChanged(int _)
        {
            RaiseUpgradeDataChanged(UpgradesWeaponSlot.First, UpgradesPanelType.Range);
        }

        private void HandleFirstSpeedLevelChanged(int _)
        {
            RaiseUpgradeDataChanged(UpgradesWeaponSlot.First, UpgradesPanelType.Speed);
        }

        private void HandleSecondDamageLevelChanged(int _)
        {
            RaiseUpgradeDataChanged(UpgradesWeaponSlot.Second, UpgradesPanelType.Damage);
        }

        private void HandleSecondRangeLevelChanged(int _)
        {
            RaiseUpgradeDataChanged(UpgradesWeaponSlot.Second, UpgradesPanelType.Range);
        }

        private void HandleSecondSpeedLevelChanged(int _)
        {
            RaiseUpgradeDataChanged(UpgradesWeaponSlot.Second, UpgradesPanelType.Speed);
        }

        public UpgradesWeaponSlot ActiveUpgradeWeaponSlot => _activeUpgradeWeaponSlot;

        public void SetActiveUpgradeWeaponSlot(UpgradesWeaponSlot slot)
        {
            if (_activeUpgradeWeaponSlot == slot)
            {
                return;
            }

            _activeUpgradeWeaponSlot = slot;
            Events_Upgrades.OnActiveUpgradeWeaponSlotChanged?.Invoke(slot);
        }

        public void UpgradeActiveWeapon(UpgradesPanelType upgradesPanelType)
        {
            bool isFirstWeapon = _activeUpgradeWeaponSlot == UpgradesWeaponSlot.First;
            int seed = _seeds != null ? Mathf.Max(0, _seeds.Value) : 0;

            if (isFirstWeapon)
            {
                if (FirstWeaponConfig() == null)
                {
                    Debug.Log("Choose a weapon first.");
                    return;
                }

                bool success = upgradesPanelType switch
                {
                    UpgradesPanelType.Damage => UpgradeFirstWeaponDamage(seed),
                    UpgradesPanelType.Range => UpgradeFirstWeaponRange(seed),
                    UpgradesPanelType.Speed => UpgradeFirstWeaponSpeed(seed),
                    _ => false
                };

                if (success)
                {
                    CloseUpgradePanelAnimated();
                }

                return;
            }

            if (SecondWeaponConfig() == null)
            {
                Debug.Log("Choose a weapon first.");
                return;
            }

            bool secondSuccess = upgradesPanelType switch
            {
                UpgradesPanelType.Damage => UpgradeSecondWeaponDamage(seed),
                UpgradesPanelType.Range => UpgradeSecondWeaponRange(seed),
                UpgradesPanelType.Speed => UpgradeSecondWeaponSpeed(seed),
                _ => false
            };

            if (secondSuccess)
            {
                CloseUpgradePanelAnimated();
            }
        }

        private void SetSeedsClamped(int value)
        {
            if (_seeds == null)
            {
                return;
            }

            _seeds.Value = Mathf.Max(0, value);
        }

        public int GetUpgradeLevel(UpgradesWeaponSlot slot, UpgradesPanelType upgradesPanelType)
        {
            IntVariable levels = ResolveLevelsVariable(slot, upgradesPanelType);
            if (levels == null)
            {
                return 1;
            }

            return Mathf.Max(1, levels.Value);
        }

        public int GetUpgradePrice(UpgradesWeaponSlot slot, UpgradesPanelType upgradesPanelType)
        {
            return (slot, upgradesPanelType) switch
            {
                (UpgradesWeaponSlot.First, UpgradesPanelType.Damage) => _firstPriceDamageUpgrade,
                (UpgradesWeaponSlot.First, UpgradesPanelType.Range) => _firstPriceRangeUpgrade,
                (UpgradesWeaponSlot.First, UpgradesPanelType.Speed) => _firstPriceSpeedUpgrade,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Damage) => _secondPriceDamageUpgrade,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Range) => _secondPriceRangeUpgrade,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Speed) => _secondPriceSpeedUpgrade,
                _ => 0
            };
        }

        public float GetUpgradePercentage(UpgradesWeaponSlot slot, UpgradesPanelType upgradesPanelType)
        {
            return (slot, upgradesPanelType) switch
            {
                (UpgradesWeaponSlot.First, UpgradesPanelType.Damage) => _firstDamagePercentage,
                (UpgradesWeaponSlot.First, UpgradesPanelType.Range) => _firstRangePercentage,
                (UpgradesWeaponSlot.First, UpgradesPanelType.Speed) => _firstSpeedPercentage,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Damage) => _secondDamagePercentage,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Range) => _secondRangePercentage,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Speed) => _secondSpeedPercentage,
                _ => 0f
            };
        }

        private IntVariable ResolveLevelsVariable(UpgradesWeaponSlot slot, UpgradesPanelType upgradesPanelType)
        {
            return (slot, upgradesPanelType) switch
            {
                (UpgradesWeaponSlot.First, UpgradesPanelType.Damage) => _firstDamageLevels,
                (UpgradesWeaponSlot.First, UpgradesPanelType.Range) => _firstRangeLevels,
                (UpgradesWeaponSlot.First, UpgradesPanelType.Speed) => _firstAtkSpeedLevels,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Damage) => _secondDamageLevels,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Range) => _secondRangeLevels,
                (UpgradesWeaponSlot.Second, UpgradesPanelType.Speed) => _secondAtkSpeedLevels,
                _ => null
            };
        }

        private void CancelPanelClose()
        {
            if (_panelCloseCts == null)
            {
                return;
            }

            _panelCloseCts.Cancel();
            _panelCloseCts.Dispose();
            _panelCloseCts = null;
        }

        private void CloseUpgradePanelAnimated()
        {
            if (_upgradesCanvasGroup == null || _upgradesPanelRectTransform == null)
            {
                HideUpgradePanel(0);
                return;
            }

            CancelPanelClose();
            var closeCts = new CancellationTokenSource();
            _panelCloseCts = closeCts;
            CloseUpgradePanelAnimatedAsync(closeCts).Forget();
        }

        private async UniTaskVoid CloseUpgradePanelAnimatedAsync(CancellationTokenSource closeCts)
        {
            const float closeDuration = 0.12f;
            CancellationToken token = closeCts.Token;

            _upgradesCanvasGroup.interactable = false;
            _upgradesCanvasGroup.blocksRaycasts = false;
            _upgradesCanvasGroup.alpha = 1f;

            if (_showPanelSequence.isAlive)
            {
                _showPanelSequence.Stop();
            }

            if (_closePanelTween.isAlive)
            {
                _closePanelTween.Stop();
            }

            _closePanelTween = Tween.Scale(_upgradesPanelRectTransform, Vector3.one * 0.92f, closeDuration, Ease.InBack);

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(closeDuration), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                if (ReferenceEquals(_panelCloseCts, closeCts))
                {
                    _panelCloseCts = null;
                }

                closeCts.Dispose();
                return;
            }

            if (token.IsCancellationRequested)
            {
                if (ReferenceEquals(_panelCloseCts, closeCts))
                {
                    _panelCloseCts = null;
                }

                closeCts.Dispose();
                return;
            }

            _upgradesCanvasGroup.alpha = 0f;
            _upgradesPanelRectTransform.localScale = Vector3.one;

            if (ReferenceEquals(_panelCloseCts, closeCts))
            {
                _panelCloseCts = null;
            }

            closeCts.Dispose();
        }

        public WeaponConfig FirstWeaponConfig()
        {
            if (_firstWeaponConfig == null)
            {
                return null;
            }
            return _firstWeaponConfig;
        }

        public WeaponConfig SecondWeaponConfig()
        {
            if (_secondWeaponConfig == null)
            {
                return null;
            }

            return _secondWeaponConfig;
        }

        private void ShowUpgradePanel(int _)
        {
            if (_upgradesCanvasGroup == null)
            {
                return;
            }

            CancelPanelClose();

            if (_closePanelTween.isAlive)
            {
                _closePanelTween.Stop();
            }

            _upgradesCanvasGroup.alpha = 1f;
            _upgradesCanvasGroup.interactable = true;
            _upgradesCanvasGroup.blocksRaycasts = true;

            if (_upgradesPanelRectTransform == null)
            {
                return;
            }

            if (_showPanelSequence.isAlive)
            {
                _showPanelSequence.Stop();
            }

            _upgradesPanelRectTransform.localScale = Vector3.one * 0.9f;
            _showPanelSequence = Tween.Scale(_upgradesPanelRectTransform, Vector3.one * 1.05f, 0.22f, Ease.OutBack)
                .Chain(Tween.Scale(_upgradesPanelRectTransform, Vector3.one, 0.14f, Ease.InOutSine));
        }

        private void HideUpgradePanel(int _)
        {
            if (_upgradesCanvasGroup == null)
            {
                return;
            }

            CancelPanelClose();

            if (_closePanelTween.isAlive)
            {
                _closePanelTween.Stop();
            }

            if (_showPanelSequence.isAlive)
            {
                _showPanelSequence.Stop();
            }

            _upgradesCanvasGroup.alpha = 0f;
            _upgradesCanvasGroup.interactable = false;
            _upgradesCanvasGroup.blocksRaycasts = false;

            if (_upgradesPanelRectTransform != null)
            {
                _upgradesPanelRectTransform.localScale = Vector3.one;
            }
        }

        #region First Weapon Upgrade
         
        public bool UpgradeFirstWeaponDamage(int seed)
        { 
            
            if (_firstPriceDamageUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return false;
            }

            if (_firstDamageLevels.Value >= _maxLevel)
            {
                Debug.Log("Max Level Upgrade");
                return false;
            }

            if (_firstDamageLevels.Value <= 0)
            {
                _firstDamageLevels.Value = 1;
            }
            
            int remaining = seed - _firstPriceDamageUpgrade;
            SetSeedsClamped(remaining);
            
            _firstWeaponConfig.WeaponDamage = (_firstWeaponConfig.WeaponDamage + (_firstWeaponInitialDamage * _firstDamagePercentage));
            _firstWeaponConfig.WeaponPierce = (_firstWeaponConfig.WeaponPierce + Mathf.RoundToInt(_firstWeaponInitialPierce * _firstDamagePercentage));
            _firstPriceDamageUpgrade += (int)(_firstPriceDamageUpgrade * _firstDamagePercentage);
            _firstDamagePercentage += _firstDamagePercentage;
            _firstDamageLevels.Value++;
            return true;
             
        }
         
        public bool UpgradeFirstWeaponRange(int seed)
        { 
            
            if (_firstPriceRangeUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return false;
            }

            if (_firstRangeLevels.Value >= _maxLevel)
            {
                Debug.Log("Max Level Upgrade");
                return false;
            }

            if (_firstRangeLevels.Value <= 0)
            {
                _firstRangeLevels.Value = 1;
            }
            
            int remaining = seed - _firstPriceRangeUpgrade;
            SetSeedsClamped(remaining);
            
            _firstWeaponConfig.WeaponRange = (_firstWeaponConfig.WeaponRange  + (_firstWeaponInitialRange * _firstRangePercentage));
            _firstWeaponConfig.WeaponKnockback = (_firstWeaponConfig.WeaponKnockback + Mathf.RoundToInt(_firstWeaponInitialKnockback * _firstRangePercentage));
            _firstPriceRangeUpgrade += (int)(_firstPriceRangeUpgrade * _firstRangePercentage);
            _firstRangePercentage += _firstRangePercentage;
            _firstRangeLevels.Value++;
            return true;
             
        }
         
        public bool UpgradeFirstWeaponSpeed(int seed)
        { 
            
            if (_firstPriceSpeedUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return false;
            }

            if (_firstAtkSpeedLevels.Value > _maxLevel)
            {
                Debug.Log("Max Level Upgrade");
                return false;
            }
             
            if (_firstAtkSpeedLevels.Value <= 0)
            {
                _firstAtkSpeedLevels.Value = 1;
            }
            
            int remaining = seed - _firstPriceSpeedUpgrade;
            SetSeedsClamped(remaining);
            
            _firstWeaponConfig.WeaponSpeed = (_firstWeaponConfig.WeaponSpeed  + (_firstWeaponInitialSpeed * _firstSpeedPercentage));
            _firstWeaponConfig.WeaponAtkSpeed = (_firstWeaponConfig.WeaponAtkSpeed + Mathf.RoundToInt(_firstWeaponInitialAtkSpeed * _firstSpeedPercentage));
            _firstPriceSpeedUpgrade += (int)(_firstPriceSpeedUpgrade * _firstSpeedPercentage);
            _firstSpeedPercentage += _firstSpeedPercentage;
            _firstAtkSpeedLevels.Value++;
            return true;
        }
         
        #endregion

        #region Second Weapon Upgrade

        public bool UpgradeSecondWeaponDamage(int seed)
        { 
            
            if (_secondPriceDamageUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return false;
            }

            if (_secondDamageLevels.Value >= _maxLevel)
            {
                Debug.Log("Max Level Upgrade");
                return false;
            }
             
            if (_secondDamageLevels.Value <= 0)
            {
                _secondDamageLevels.Value = 1;
            }
            
            int remaining = seed - _secondPriceDamageUpgrade;
            SetSeedsClamped(remaining);
            
            _secondWeaponConfig.WeaponDamage = (_secondWeaponConfig.WeaponDamage + (_secondWeaponInitialDamage * _secondDamagePercentage));
            _secondWeaponConfig.WeaponPierce = (_secondWeaponConfig.WeaponPierce + Mathf.RoundToInt(_secondWeaponInitialPierce * _secondDamagePercentage));
            _secondPriceDamageUpgrade += (int)(_firstPriceDamageUpgrade * _secondDamagePercentage);
            _secondDamagePercentage += _secondDamagePercentage;
            _secondDamageLevels.Value++;
            return true;
             
        }
         
        public bool UpgradeSecondWeaponRange(int seed)
        { 
            
            if (_secondPriceRangeUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return false;
            }

            if (_secondRangeLevels.Value >= _maxLevel)
            {
                Debug.Log("Max Level Upgrade");
                return false;
            }

            if (_secondRangeLevels.Value <= 0)
            {
                _secondRangeLevels.Value = 1;
            }
            
            int remaining = seed - _secondPriceRangeUpgrade;
            SetSeedsClamped(remaining);
            
            _secondWeaponConfig.WeaponRange = (_secondWeaponConfig.WeaponRange  + (_secondWeaponInitialRange * _secondRangePercentage));
            _secondWeaponConfig.WeaponKnockback = (_secondWeaponConfig.WeaponKnockback + Mathf.RoundToInt(_secondWeaponInitialKnockback * _secondRangePercentage));
            _secondPriceRangeUpgrade += (int)(_secondPriceRangeUpgrade * _secondRangePercentage);
            _secondRangePercentage += _secondRangePercentage;
            _secondRangeLevels.Value++;
            return true;
             
        }
         
        public bool UpgradeSecondWeaponSpeed(int seed)
        { 
            
            if (_secondPriceSpeedUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return false;
            }

            if (_secondAtkSpeedLevels.Value >= _maxLevel)
            {
                Debug.Log("Max Level Upgrade");
                return false;
            }
             
            if (_secondAtkSpeedLevels.Value <= 0)
            {
                _secondAtkSpeedLevels.Value = 1;
            }
            
            int remaining = seed - _secondPriceSpeedUpgrade;
            SetSeedsClamped(remaining);
            
            _secondWeaponConfig.WeaponSpeed = (_secondWeaponConfig.WeaponSpeed  + (_secondWeaponInitialSpeed * _secondSpeedPercentage));
            _secondWeaponConfig.WeaponAtkSpeed = (_secondWeaponConfig.WeaponAtkSpeed + Mathf.RoundToInt(_secondWeaponInitialAtkSpeed * _secondSpeedPercentage));
            _secondPriceSpeedUpgrade += (int)(_secondPriceSpeedUpgrade * _firstSpeedPercentage);
            _secondSpeedPercentage += _secondSpeedPercentage;
            _secondAtkSpeedLevels.Value++;
            return true;
        }

        #endregion
        private async void CurrentWeapon(string label)
        {
            var handle = Addressables.LoadAssetAsync<WeaponConfig>(label); // label
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                InitializeWeaponConfig(handle.Result);
            }
            else
            {
                Debug.LogError("Failed to load WeaponConfig with label 'Spear'");
            }
        }

        private void InitializeWeaponConfig(WeaponConfig weaponConfig)
        {
            if (!_isFirstOccupied)
            {
                _firstWeaponConfig = weaponConfig;
                SetUpFirstWeaponConfig();
                Events_Upgrades.OnChosenWeapon?.Invoke(_firstWeaponConfig, true);
                _isFirstOccupied = true;
            }
            else if (!_isSecondOccupied)
            {
                _secondWeaponConfig = weaponConfig;
                SetUpSecondWeaponConfig();
                Events_Upgrades.OnChosenWeapon?.Invoke(_secondWeaponConfig, false);
                _isSecondOccupied = true;
            }
            else
            {
                Debug.Log("both arms are occupied");
            }
        }

        private void SetUpFirstWeaponConfig()
        {
            _firstWeaponInitialDamage = _firstWeaponConfig.WeaponDamage;
            _firstWeaponInitialPierce = _firstWeaponConfig.WeaponPierce;
            _firstWeaponInitialRange = _firstWeaponConfig.WeaponRange;
            _firstWeaponInitialKnockback = _firstWeaponConfig.WeaponKnockback;
            _firstWeaponInitialSpeed = _firstWeaponConfig.WeaponSpeed;
            _firstWeaponInitialAtkSpeed = _firstWeaponConfig.WeaponAtkSpeed;
        }
        
        private void SetUpSecondWeaponConfig()
        {
            _secondWeaponInitialDamage = _secondWeaponConfig.WeaponDamage;
            _secondWeaponInitialPierce = _secondWeaponConfig.WeaponPierce;
            _secondWeaponInitialRange = _secondWeaponConfig.WeaponRange;
            _secondWeaponInitialKnockback = _secondWeaponConfig.WeaponKnockback;
            _secondWeaponInitialSpeed = _secondWeaponConfig.WeaponSpeed;
            _secondWeaponInitialAtkSpeed = _secondWeaponConfig.WeaponAtkSpeed;
        }
         
    }

}
