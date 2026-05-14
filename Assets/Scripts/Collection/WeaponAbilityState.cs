using System.Threading;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

namespace Collection
{
    public abstract class WeaponAbilityState : MonoBehaviour
    {
        [SerializeField] protected WeaponConfig _weaponConfig;
        [Range(1f,5f)] [SerializeField] protected float _percentageIncrease;
        protected CancellationTokenSource _abilityCts;
        protected CancellationTokenSource _cooldownCts;

        public abstract void UseWeaponAbility();
        public abstract UniTask UseWeaponAbilityAsync(CancellationToken token);
        public abstract UniTask WeaponAbilityCooldown(CancellationToken token);
    }
}
