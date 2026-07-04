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
