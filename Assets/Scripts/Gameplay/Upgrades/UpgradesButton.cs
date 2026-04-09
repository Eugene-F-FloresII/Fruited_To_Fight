using Controllers;
using Data;
using Obvious.Soap;
using Shared.Enums;
using Shared.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Upgrades
{
    public class UpgradesButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Upgrades Panel Config")]
        [SerializeField] private UpgradesPanelConfig _upgradesPanelConfig;
        [SerializeField] private UpgradesController _upgradesController;
        [SerializeField] private UpgradesPanelType _upgradesPanelType;
        [SerializeField] private bool _isFirstWeapon;
        [SerializeField] private IntVariable _seed;

        private WeaponConfig _firstWeaponConfig;
        private WeaponConfig _secondWeaponConfig;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _firstWeaponConfig = _upgradesController.FirstWeaponConfig();
            _secondWeaponConfig = _upgradesController.SecondWeaponConfig();
            
            
            Events_Upgrades.OnHoveredUpgrade?.Invoke(_upgradesPanelConfig.UpgradesPanelType, _upgradesPanelConfig.IsFirstWeapon);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
             if (_isFirstWeapon && _upgradesPanelType == UpgradesPanelType.Damage)
             {
                 _upgradesController.UpgradeFirstWeaponDamage(_seed.Value);
             } else if (_isFirstWeapon && _upgradesPanelType == UpgradesPanelType.Range)
             {
                 _upgradesController.UpgradeFirstWeaponRange(_seed.Value);
                            
             } else if (_isFirstWeapon && _upgradesPanelType == UpgradesPanelType.Speed)
             {
                 _upgradesController.UpgradeFirstWeaponSpeed(_seed.Value);
             }
             else if(!_isFirstWeapon && _upgradesPanelType == UpgradesPanelType.Damage)
             {
                 _upgradesController.UpgradeSecondWeaponDamage(_seed.Value);
             }else if(!_isFirstWeapon && _upgradesPanelType == UpgradesPanelType.Range)
             {
                 _upgradesController.UpgradeSecondWeaponRange(_seed.Value);
             } else if(!_isFirstWeapon && _upgradesPanelType == UpgradesPanelType.Speed)
             {
                 _upgradesController.UpgradeSecondWeaponSpeed(_seed.Value);
             }
             
        }
    }

}
