using Controllers;
using Shared.Enums;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Upgrades
{
    public class UpgradesWeaponSlotSelector : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private UpgradesWeaponSlot _slotToSelect = UpgradesWeaponSlot.First;
        [SerializeField] private UpgradesController _upgradesController;

        private void Awake()
        {
            if (_upgradesController == null)
            {
                _upgradesController = GetComponentInParent<UpgradesController>(true);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_upgradesController == null)
            {
                Debug.LogError($"{nameof(UpgradesWeaponSlotSelector)}: Missing {nameof(UpgradesController)} reference.");
                return;
            }

            _upgradesController.SetActiveUpgradeWeaponSlot(_slotToSelect);
        }
    }
}

