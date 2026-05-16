using Controllers;
using Data;
using Shared.Enums;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class Spear : ProjectileWeapon
    {
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            { 
                enemy.TakeDamage(CurrentDamage, this);
                
                if (_weaponConfig.Afflictions != null)
                {
                    foreach (var affliction in _weaponConfig.Afflictions)
                    {
                        enemy.ApplyAffliction(affliction);
                    }
                }
                CurrentPierce--;
                
                OnPierceValueChanged();
                
                if (CurrentPierce <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public override void RefreshAfflictionVisuals()
        {
            if (_weaponAffliction == null || _weaponConfig == null || _weaponConfig.Afflictions == null)
            {
                return;
            }

            _weaponAffliction.DisableAllVisuals();

            if (_weaponConfig.Afflictions.Count > 0)
            {
                var primaryAffliction = _weaponConfig.Afflictions[0];
                _weaponAffliction.ToggleVisual(primaryAffliction.Type, true);
            }
        }
    }

}
