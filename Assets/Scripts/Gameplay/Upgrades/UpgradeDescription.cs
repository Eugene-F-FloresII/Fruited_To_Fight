using System.Threading;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;
using Shared.Events;
using TMPro;

namespace Gameplay.Upgrades
{
    public class UpgradeDescription : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private float _textSpeed;

        private CancellationTokenSource _descriptionCts;
        private WeaponConfig _firstWeaponConfig;
        private WeaponConfig _secondWeaponConfig;

        private void OnEnable()
        {
            Events_Upgrades.OnHoveredUpgrade += CatchDescription;
            Events_Upgrades.OnChosenWeapon += CacheWeapon;
        }

        private void OnDisable()
        {
            Events_Upgrades.OnHoveredUpgrade -= CatchDescription;
            Events_Upgrades.OnChosenWeapon -= CacheWeapon;
            
            _descriptionCts?.Cancel();
            _descriptionCts?.Dispose();
        }

        private void OnDestroy()
        {
            _descriptionCts?.Cancel();
            _descriptionCts?.Dispose();
        }

        private void CacheWeapon(WeaponConfig weaponConfig, bool isFirstWeapon)
        {
            if (isFirstWeapon)
            {
                _firstWeaponConfig = weaponConfig;
                return;
            }

            _secondWeaponConfig = weaponConfig;
        }

        private void CatchDescription(UpgradesPanelType upgradesPanelType, bool isFirstWeapon)
        {
            var weaponConfig = isFirstWeapon ? _firstWeaponConfig : _secondWeaponConfig;
            var description = ResolveDescription(weaponConfig, upgradesPanelType);

            _descriptionCts?.Cancel();
            _descriptionCts?.Dispose();
            _descriptionCts = new CancellationTokenSource();
            
            UpdateDescription(description, _descriptionCts.Token).Forget();
        }

        private string ResolveDescription(WeaponConfig weaponConfig, UpgradesPanelType upgradesPanelType)
        {
            if (weaponConfig == null)
            {
                return "Choose a weapon first.";
            }

            string description = upgradesPanelType switch
            {
                UpgradesPanelType.Damage => weaponConfig.DamageDescription,
                UpgradesPanelType.Range => weaponConfig.RangeDescription,
                UpgradesPanelType.Speed => weaponConfig.SpeedDescription,
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(description))
            {
                return "No upgrade description available.";
            }

            return description;
        }

        private async UniTask UpdateDescription(string description, CancellationToken token)
        {
            _descriptionText.text = string.Empty; // Clear first

            int delayMs = Mathf.RoundToInt(1000f / _textSpeed); // _textSpeed = chars per second

            foreach (char c in description)
            {
                if (token.IsCancellationRequested) return;

                _descriptionText.text += c;
                
                await UniTask.Delay(delayMs, cancellationToken: token);
            }
        }
    }

}
