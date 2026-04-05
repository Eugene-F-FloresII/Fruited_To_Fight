using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Events;

namespace Gameplay.Weapons
{
    public class WeaponPicker : MonoBehaviour
    {
        private void Start()
        {
            Time.timeScale = 0;
        }

        public void OnPickedSpear()
        {
            Events_Weapons.OnChosenWeapon?.Invoke("SpearConfig");
            Time.timeScale = 1;
        }
        
    }

}
