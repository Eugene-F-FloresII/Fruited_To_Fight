using Controllers;
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
    }

}
