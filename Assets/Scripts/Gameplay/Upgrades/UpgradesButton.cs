using Data;
using Shared.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Upgrades
{
    public class UpgradesButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Upgrades Panel Config")]
        [SerializeField] private UpgradesPanelConfig _upgradesPanelConfig;
        
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            Events_Upgrades.OnHoveredUpgrade?.Invoke(_upgradesPanelConfig.UpgradesPanelType, _upgradesPanelConfig.IsFirstWeapon);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
        }
    }

}
