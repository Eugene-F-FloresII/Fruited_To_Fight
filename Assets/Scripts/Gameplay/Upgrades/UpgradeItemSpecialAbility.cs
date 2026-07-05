using Controllers;
using Data;
using TMPro;
using UnityEngine;

namespace Gameplay.Upgrades
{
    public class UpgradeItemSpecialAbility : UpgradeItemController
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponConfig _weaponConfig;
        [SerializeField] private float _statUpgradePercentage = 0.1f; // 10% per level

        private float _baseCooldown;
        private float _baseDuration;

        protected override void Awake()
        {
            base.Awake();
            InitializeBaseStats();
        }

        private void InitializeBaseStats()
        {
            if (_weaponConfig == null) return;

            int currentLvl = (_levelVariable != null) ? _levelVariable.Value : 1;
            if (currentLvl < 1) currentLvl = 1;

            float cooldown = _weaponConfig.AbilityCooldown;
            float duration = _weaponConfig.AbilityDuration;

            for (int i = currentLvl; i > 1; i--)
            {
                if (!Mathf.Approximately(1f - _statUpgradePercentage, 0f))
                {
                    cooldown /= (1f - _statUpgradePercentage);
                }
                if (!Mathf.Approximately(1f + _statUpgradePercentage, 0f))
                {
                    duration /= (1f + _statUpgradePercentage);
                }
            }

            _baseCooldown = cooldown;
            _baseDuration = duration;
        }

        public override void ResetUpgrade()
        {
            if (_weaponConfig != null)
            {
                _weaponConfig.AbilityCooldown = _baseCooldown;
                _weaponConfig.AbilityDuration = _baseDuration;
            }
            base.ResetUpgrade();
        }

        [Header("Current Special Ability Settings")]
        [SerializeField] private TextMeshProUGUI _currentCooldownText;
        [SerializeField] private TextMeshProUGUI _currentDurationText;
        
        [Header("Future Special Ability Settings")]
        [SerializeField] private TextMeshProUGUI _futureCooldownText;
        [SerializeField] private TextMeshProUGUI _futureDurationText;

        public override void UpdateUpgradeCost()
        {
            base.UpdateUpgradeCost();
            UpdateSpecialAbilityTexts();
        }

        public override void OnBoughtUpgradeItem()
        {
            if (_weaponConfig == null) return;

            // 1. Apply permanent stats upgrade
            ApplyPermanentStatsUpgrade();

            // 2. Update texts to show the newly upgraded stats
            UpdateSpecialAbilityTexts();
        }

        private void UpdateSpecialAbilityTexts()
        {
            if (_weaponConfig == null) return;

            // Update Current texts
            if (_currentCooldownText != null) _currentCooldownText.text = _weaponConfig.AbilityCooldown.ToString("F1");
            if (_currentDurationText != null) _currentDurationText.text = _weaponConfig.AbilityDuration.ToString("F1");

            // Update Future (next level preview) texts
            UpdateFutureSpecialAbilityTexts();
        }

        private void UpdateFutureSpecialAbilityTexts()
        {
            if (_weaponConfig == null) return;

            if (_currentLevel >= (_maxStars + 1))
            {
                if (_futureCooldownText != null) _futureCooldownText.text = "-";
                if (_futureDurationText != null) _futureDurationText.text = "-";
                return;
            }

            if (_futureCooldownText != null) _futureCooldownText.text = GetFutureCooldown().ToString("F1");
            if (_futureDurationText != null) _futureDurationText.text = GetFutureDuration().ToString("F1");
        }

        private float GetFutureCooldown() => _weaponConfig.AbilityCooldown * (1f - _statUpgradePercentage);
        private float GetFutureDuration() => _weaponConfig.AbilityDuration * (1f + _statUpgradePercentage);

        private void ApplyPermanentStatsUpgrade()
        {
            if (_weaponConfig == null) return;

            _weaponConfig.AbilityCooldown = GetFutureCooldown();
            _weaponConfig.AbilityDuration = GetFutureDuration();
        }
    }
}
