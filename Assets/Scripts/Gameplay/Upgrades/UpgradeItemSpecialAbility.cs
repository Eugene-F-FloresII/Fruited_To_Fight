using Controllers;
using Data;
using TMPro;
using UnityEngine;

namespace Gameplay.Upgrades
{
    /// <summary>
    /// Handles the permanent upgrade logic for special/ultimate abilities of weapons on the Main Menu.
    /// Scales cooldown, duration, radius, spawn count, and speed per level.
    /// </summary>
    public class UpgradeItemSpecialAbility : UpgradeItemController
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponConfig _weaponConfig;
        [SerializeField] private float _statUpgradePercentage = 0.1f; // 10% per level

        private float _baseCooldown;
        private float _baseDuration;
        private float _baseRadius;
        private int _baseSpawnCount;
        private float _baseSpeed;

        [Header("Current Special Ability Settings")]
        [SerializeField] private TextMeshProUGUI _currentCooldownText;
        [SerializeField] private TextMeshProUGUI _currentDurationText;
        
        [Header("Current Special Ability Extended Settings")]
        [SerializeField] private TextMeshProUGUI _currentRadiusText;
        [SerializeField] private TextMeshProUGUI _currentSpawnCountText;
        [SerializeField] private TextMeshProUGUI _currentSpeedText;
        
        [Header("Future Special Ability Settings")]
        [SerializeField] private TextMeshProUGUI _futureCooldownText;
        [SerializeField] private TextMeshProUGUI _futureDurationText;

        [Header("Future Special Ability Extended Settings")]
        [SerializeField] private TextMeshProUGUI _futureRadiusText;
        [SerializeField] private TextMeshProUGUI _futureSpawnCountText;
        [SerializeField] private TextMeshProUGUI _futureSpeedText;

        protected override void Awake()
        {
            base.Awake();
            InitializeBaseStats();
        }

        /// <summary>
        /// Calculates the base values (level 1 values) of all ability stats by reversing the upgrade calculations.
        /// </summary>
        private void InitializeBaseStats()
        {
            if (_weaponConfig == null) return;

            int currentLvl = (_levelVariable != null) ? _levelVariable.Value : 1;
            if (currentLvl < 1) currentLvl = 1;

            float cooldown = _weaponConfig.AbilityCooldown;
            float duration = _weaponConfig.AbilityDuration;
            float radius = _weaponConfig.AbilityRadius;
            int spawnCount = _weaponConfig.AbilitySpawnCount;
            float speed = _weaponConfig.AbilitySpeed;

            for (int levelIndex = currentLvl; levelIndex > 1; levelIndex--)
            {
                if (!Mathf.Approximately(1f - _statUpgradePercentage, 0f))
                {
                    cooldown /= (1f - _statUpgradePercentage);
                }
                if (!Mathf.Approximately(1f + _statUpgradePercentage, 0f))
                {
                    duration /= (1f + _statUpgradePercentage);
                    radius /= (1f + _statUpgradePercentage);
                    speed /= (1f + _statUpgradePercentage);
                }
                spawnCount = Mathf.Max(1, spawnCount - 1);
            }

            _baseCooldown = cooldown;
            _baseDuration = duration;
            _baseRadius = radius;
            _baseSpawnCount = spawnCount;
            _baseSpeed = speed;
        }

        /// <summary>
        /// Resets all upgraded weapon ability stats to their original base levels.
        /// </summary>
        public override void ResetUpgrade()
        {
            if (_weaponConfig != null)
            {
                _weaponConfig.AbilityCooldown = _baseCooldown;
                _weaponConfig.AbilityDuration = _baseDuration;
                _weaponConfig.AbilityRadius = _baseRadius;
                _weaponConfig.AbilitySpawnCount = _baseSpawnCount;
                _weaponConfig.AbilitySpeed = _baseSpeed;
            }
            base.ResetUpgrade();
        }

        /// <summary>
        /// Updates the upgrade cost and syncs UI texts showing current and next level preview stats.
        /// </summary>
        public override void UpdateUpgradeCost()
        {
            base.UpdateUpgradeCost();
            UpdateSpecialAbilityTexts();
        }

        /// <summary>
        /// Callback invoked after a successful upgrade purchase. Applies changes and updates text.
        /// </summary>
        public override void OnBoughtUpgradeItem()
        {
            if (_weaponConfig == null) return;

            // 1. Apply permanent stats upgrade
            ApplyPermanentStatsUpgrade();

            // 2. Update texts to show the newly upgraded stats
            UpdateSpecialAbilityTexts();
        }

        /// <summary>
        /// Updates all UI text components with the current values of the special ability.
        /// </summary>
        private void UpdateSpecialAbilityTexts()
        {
            if (_weaponConfig == null) return;

            // Update Current texts
            if (_currentCooldownText != null) _currentCooldownText.text = _weaponConfig.AbilityCooldown.ToString("F1");
            if (_currentDurationText != null) _currentDurationText.text = _weaponConfig.AbilityDuration.ToString("F1");
            if (_currentRadiusText != null) _currentRadiusText.text = _weaponConfig.AbilityRadius.ToString("F1");
            if (_currentSpawnCountText != null) _currentSpawnCountText.text = _weaponConfig.AbilitySpawnCount.ToString();
            if (_currentSpeedText != null) _currentSpeedText.text = _weaponConfig.AbilitySpeed.ToString("F1");

            // Update Future (next level preview) texts
            UpdateFutureSpecialAbilityTexts();
        }

        /// <summary>
        /// Updates all UI text components representing previews of the next upgrade level.
        /// </summary>
        private void UpdateFutureSpecialAbilityTexts()
        {
            if (_weaponConfig == null) return;

            if (_currentLevel >= (_maxStars + 1))
            {
                if (_futureCooldownText != null) _futureCooldownText.text = "-";
                if (_futureDurationText != null) _futureDurationText.text = "-";
                if (_futureRadiusText != null) _futureRadiusText.text = "-";
                if (_futureSpawnCountText != null) _futureSpawnCountText.text = "-";
                if (_futureSpeedText != null) _futureSpeedText.text = "-";
                return;
            }

            if (_futureCooldownText != null) _futureCooldownText.text = GetFutureCooldown().ToString("F1");
            if (_futureDurationText != null) _futureDurationText.text = GetFutureDuration().ToString("F1");
            if (_futureRadiusText != null) _futureRadiusText.text = GetFutureRadius().ToString("F1");
            if (_futureSpawnCountText != null) _futureSpawnCountText.text = GetFutureSpawnCount().ToString();
            if (_futureSpeedText != null) _futureSpeedText.text = GetFutureSpeed().ToString("F1");
        }

        private float GetFutureCooldown() => _weaponConfig.AbilityCooldown * (1f - _statUpgradePercentage);
        private float GetFutureDuration() => _weaponConfig.AbilityDuration * (1f + _statUpgradePercentage);
        private float GetFutureRadius() => _weaponConfig.AbilityRadius * (1f + _statUpgradePercentage);
        private int GetFutureSpawnCount() => _weaponConfig.AbilitySpawnCount + 1;
        private float GetFutureSpeed() => _weaponConfig.AbilitySpeed * (1f + _statUpgradePercentage);

        /// <summary>
        /// Modifies the WeaponConfig permanently to apply the new upgraded ability statistics.
        /// </summary>
        private void ApplyPermanentStatsUpgrade()
        {
            if (_weaponConfig == null) return;

            _weaponConfig.AbilityCooldown = GetFutureCooldown();
            _weaponConfig.AbilityDuration = GetFutureDuration();
            _weaponConfig.AbilityRadius = GetFutureRadius();
            _weaponConfig.AbilitySpawnCount = GetFutureSpawnCount();
            _weaponConfig.AbilitySpeed = GetFutureSpeed();
        }
    }
}
