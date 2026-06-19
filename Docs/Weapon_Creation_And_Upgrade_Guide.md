# Guide to Creating and Upgrading Projectile Weapons

This guide explains how to create a new **Projectile Weapon** in the **Fruited to Fight** project, from the initial script creation to hookups for special abilities, in-game upgrades, and elemental afflictions. 

We will use the **Tomahawk** weapon as our reference because it implements all these systems (spawning projectile fans, specialized spinning ability, and upgrades).

---

## Table of Contents
1. [Weapon Architecture Overview](#1-weapon-architecture-overview)
2. [Step 1: Define Enums](#step-1-define-enums)
3. [Step 2: C# Script Implementations](#step-2-c-script-implementations)
4. [Step 3: Create Weapon & Projectile Prefabs](#step-3-create-weapon--projectile-prefabs)
5. [Step 4: Create Scriptable Objects](#step-4-create-scriptable-objects)
6. [Step 5: Addressables Setup](#step-5-addressables-setup)
7. [Step 6: Register in Managers and Controllers](#step-6-register-in-managers-and-controllers)
8. [Step 7: UI Integration](#step-7-ui-integration)

---

## 1. Weapon Architecture Overview

A projectile weapon in this project consists of several decoupled parts:
- **Projectile Script**: Inherits from [ProjectileWeapon](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileWeapon.cs). It defines how the individual projectile interacts with enemies (damage, collision, pierce counts, homing movement, and despawn timers).
- **Spawner Script**: Inherits from [ProjectileSpawner](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileSpawner.cs). It handles detecting enemies in range and controlling the custom async firing loop.
- **Ability State**: Inherits from [WeaponAbilityState](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Collection/WeaponAbilityState.cs). This component is attached to the player and handles the weapon's special/ultimate ability.
- **Configurations (ScriptableObjects)**: 
  - [WeaponConfig](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Data/WeaponConfig.cs) defines the base statistics and prefab references.
  - [UpgradeWeapon](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Data/Upgrades/UpgradeWeapon.cs) defines the shop upgrades.
  - [UpgradeAfflictionData](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Data/Upgrades/UpgradeAfflictionData.cs) links weapon classes to elemental status effects.

---

## Step 1: Define Enums

Every new weapon must be registered in the shared enums:

1. Open [WeaponClass.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Shared/Enums/WeaponClass.cs) and add your weapon's class:
   ```csharp
   public enum WeaponClass
   {
       None,
       Spear,
       Tomahawk,
       Staff,
       Sword,
       YourNewWeapon // <-- Add here
   }
   ```

2. Open [UpgradesPanel.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Shared/Enums/UpgradesPanel.cs) and add your weapon's upgrade category type:
   ```csharp
   public enum UpgradesCategoryType
   {
       Damage,
       Pierce,
       Range,
       Knockback,
       Speed,
       AttackSpeed,
       Tomahawk,
       LightningWisp,
       YourNewWeapon // <-- Add here
   }
   ```

---

## Step 2: C# Script Implementations

Create your weapon scripts under `Assets/Scripts/Gameplay/Weapons/`.

### A. The Projectile Script
For a projectile weapon, inherit from [ProjectileWeapon](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileWeapon.cs). If no custom projectile logic is needed, the class can be empty since `ProjectileWeapon` handles collision detection, damage, homing, and despawn timers.

Reference: [Tomahawk.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/Tomahawk.cs)
```csharp
using UnityEngine;

namespace Gameplay.Weapons
{
    public class YourNewWeapon : ProjectileWeapon
    {
        // Add custom projectile logic here if needed (e.g., piercing effects, special trails)
    }
}
```

### B. The Spawner Script
Inherit from [ProjectileSpawner](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileSpawner.cs) and override the `AttackEnemyAsync` method to implement custom firing patterns.

Reference: [TomahawkSpawner.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/TomahawkSpawner.cs)
```csharp
using System;
using System.Threading;
using Controllers;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Events;

namespace Gameplay.Weapons
{
    public class YourNewWeaponSpawner : ProjectileSpawner
    {
        [Header("Audio")] 
        [SerializeField] private AudioClip _audioClip;

        protected override async UniTask AttackEnemyAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    EnemyController target = GetNearestEnemy();
                    if (target == null)
                    {
                        StopAttackLoop();
                        return;
                    }

                    // Calculate direction
                    Vector2 directionToTarget = (Vector2)target.transform.position - (Vector2)transform.position;
                    float baseAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

                    // Implement level scaling projectile counts
                    int weaponLevel = _weaponConfig.WeaponLevel.Value;
                    int projectileCount = weaponLevel == 0 ? 1 : 1 + (weaponLevel + 1) / 2;

                    for (int i = 0; i < projectileCount; i++)
                    {
                        // Calculate angle offsets for fans/bursts
                        float angleOffset = 0;
                        if (i > 0)
                        {
                            int multiplier = (i + 1) / 2;
                            angleOffset = (i % 2 != 0) ? 15f * multiplier : -15f * multiplier;
                        }

                        float finalAngle = baseAngle + angleOffset;
                        Quaternion rotation = Quaternion.Euler(0, 0, finalAngle + _projectileRotationOffset);
                        
                        GameObject projectile = GetPooledObject();
                        if (projectile != null)
                        {
                            projectile.transform.position = transform.position;
                            projectile.transform.rotation = rotation;
                            projectile.SetActive(true);
                            
                            Events_Sound.PlaySound?.Invoke(_audioClip);

                            if (projectile.TryGetComponent(out Rigidbody2D rb))
                            {
                                Vector2 direction = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
                                rb.linearVelocity = direction * _weaponConfig.WeaponSpeed;
                            }
                        }
                    }
                    
                    // Attack speed delay
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0.01f, _currentAtkSpeed)), cancellationToken: token);
                }
            }
            catch (OperationCanceledException) {}
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                StopAttackLoop();
            }
        }
    }
}
```

### C. The Special Ability State
Inherit from [WeaponAbilityState](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Collection/WeaponAbilityState.cs). This controls the weapon's ultimate ability activated by UI buttons.

Reference: [TomahawkAbilityState.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/TomahawkAbilityState.cs)
```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using Collection;
using Controllers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class YourNewWeaponAbilityState : WeaponAbilityState
    {
        [Header("Ability Settings")]
        [SerializeField] private float _spinRadius = 2.5f;
        [SerializeField] private Transform _target; // Set to Player's Transform in the inspector

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<YourNewWeaponAbilityState>();
            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _cooldownCts?.Cancel();
            _cooldownCts?.Dispose();
        }

        public override void UseWeaponAbility()
        {
            if (_weaponConfig == null) return;
            
            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _abilityCts = new CancellationTokenSource();
            
            UseWeaponAbilityAsync(_abilityCts.Token).Forget();
        }

        public override async UniTask UseWeaponAbilityAsync(CancellationToken token)
        {
            try
            {
                // Custom Ability Logic (e.g. Spawn orbiters, increase attack speed, temporary shield)
                float duration = _weaponConfig.AbilityDuration;
                float startTime = Time.time;

                while (Time.time - startTime < duration && !token.IsCancellationRequested)
                {
                    if (_target == null) break;
                    
                    // Ability update tick
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) {}
            finally
            {
                // Clean up ability objects
                _cooldownCts?.Cancel();
                _cooldownCts?.Dispose();
                _cooldownCts = new CancellationTokenSource();
                WeaponAbilityCooldown(_cooldownCts.Token).Forget();
            }
        }

        public override async UniTask WeaponAbilityCooldown(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_weaponConfig.AbilityCooldown), cancellationToken: token);
            }
            catch (OperationCanceledException) {}
        }
    }
}
```

---

## Step 3: Create Weapon & Projectile Prefabs

In Unity:

### A. The Projectile Prefab
1. Create a 2D Sprite GameObject for the projectile.
2. Add your custom script (e.g. `YourNewWeapon`) which inherits from `ProjectileWeapon`.
3. Add a `Rigidbody2D` (Body Type: **Dynamic**, Collision Detection: **Continuous**, Gravity Scale: **0**).
4. Add a `Collider2D` (e.g., `CircleCollider2D` or `BoxCollider2D`) set as a **Trigger**.
5. Save the prefab inside `Assets/Prefabs/Weapon/`.

### B. The Spawner Prefab
1. Create a GameObject for the weapon itself.
2. Add your custom spawner script (e.g. `YourNewWeaponSpawner`) which inherits from `ProjectileSpawner`.
3. Add a `CircleCollider2D` set to **Trigger**. The script will automatically resize this radius to match the weapon's range.
4. Add a child GameObject named `PooledTransform` to act as the parent folder for instantiated pooled projectiles.
5. Save the prefab inside `Assets/Prefabs/WeaponPrefabs/`.

---

## Step 4: Create Scriptable Objects

Create three Scriptable Objects to configure statistics, upgrades, and afflictions:

### A. Weapon Config
1. Right-click in the Project window and select **Create -> Data -> Create Weapon Config**.
2. Save it inside `Assets/Prefabs/Data_Weapons/` (e.g. `YourNewWeaponConfig.asset`).
3. Set properties:
   - **Weapon Spawner**: Reference your Spawner prefab.
   - **Weapon Prefab**: Reference your Projectile prefab.
   - **Weapon Name**: Name of your weapon.
   - **Weapon Class**: Select your enum value from Step 1.
   - **Weapon Amount to Pool**: Recommended ~20.
   - **Weapon Level**: Create or reference a Soap `IntVariable` scriptable object.
   - Set damage, pierce, range, speed, and attack speed values.

### B. Upgrade Weapon Data
1. Right-click and select **Create -> Data -> Create Weapon Upgrade Data**.
2. Save it inside `Assets/Prefabs/Data_Upgrade/` (e.g. `YourNewWeaponUpgrade.asset`).
3. Set properties:
   - **Category**: Set to your new `UpgradesCategoryType` enum value.
   - **Percentage Increase Per Level**: Statistical multiplier per level (e.g., `0.15` for 15%).
   - **Upgrade Level**: Assign the same Soap `IntVariable` used in the Weapon Config.
   - **Max Level**: Max upgrade level.
   - **Price Upgrade**: Price increment base value.
   - **Button Prefab**: Reference the UI upgrade button prefab.

### C. Upgrade Affliction Data
1. Right-click and select **Create -> Data -> Create Upgrade Affliction Data**.
2. Save it inside `Assets/Prefabs/Data_AfflictionUpgrade/` (e.g. `Fire_YourNewWeapon.asset`).
3. Set properties:
   - **Weapon Class**: Select your enum value.
   - **Affliction Type**: The elemental type (e.g., `Burn`, `Freeze`).
   - **Affliction Key**: The Addressable key of the [AfflictionConfig](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Data/AfflictionConfig.cs) asset to load.
   - **Button Prefab**: Reference the UI affliction choice button prefab.

---

## Step 5: Addressables Setup

To enable dynamic Addressable Asset loading:

1. Select your new assets in the Project window:
   - `YourNewWeaponConfig.asset`
   - `YourNewWeaponUpgrade.asset`
   - `Fire_YourNewWeapon.asset`
   - `YourNewWeaponSpawner.prefab`
   - `YourNewWeapon.prefab`
2. Check the **Addressable** checkbox at the top of their inspector windows.
3. Open the **Addressables Groups** window (**Window -> Asset Management -> Addressables -> Groups**).
4. Rename their address keys to clean strings:
   - Config Address: `YourNewWeaponConfig`
   - Spawner Address: `YourNewWeaponSpawner`
   - Upgrade Address: `YourNewWeaponUpgrade`
   - Affliction Address: `Fire_YourNewWeapon` (if adding Fire, etc.)

---

## Step 6: Register in Managers and Controllers

Hook the scripts up to the game manager singletons:

### A. SpecialAbilityInteractable
Open [SpecialAbilityInteractable.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/SpecialAbilityInteractable.cs):
1. Declare the ability state variable:
   ```csharp
   private YourNewWeaponAbilityState _yourNewWeaponAbilityState;
   ```
2. Initialize it in `Start()`:
   ```csharp
   _yourNewWeaponAbilityState = ServiceLocator.TryGet<YourNewWeaponAbilityState>();
   ```
3. Add a case inside `UseSpecialAbility()`:
   ```csharp
   case WeaponClass.YourNewWeapon:
       _yourNewWeaponAbilityState.UseWeaponAbility();
       break;
   ```

### B. UpgradesManager
Open [UpgradesManager.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Managers/UpgradesManager.cs):
1. Add a private field for the upgrade data:
   ```csharp
   private UpgradeData _yourNewWeapon;
   ```
2. Initialize the field in `ConfigureAllUpgrades()`:
   ```csharp
   _yourNewWeapon = GetUpgrade(UpgradesCategoryType.YourNewWeapon);
   ```
3. Add an upgrade method:
   ```csharp
   public int UpgradeYourNewWeapon(int seed)
   {
       if (_yourNewWeapon.GetUpgradeLevelMaxed()) return seed;

       WeaponConfig target = _activeWeapons.FirstOrDefault(w => w.WeaponClass == WeaponClass.YourNewWeapon);
       if (target == null) return seed;

       UpgradeWeaponResult result = _yourNewWeapon.BuyWeaponUpgrade(seed, target.WeaponDamage, target.WeaponSpeed, target.WeaponRange);
       
       foreach (var weapon in _activeWeapons.Where(w => w.WeaponClass == WeaponClass.YourNewWeapon))
       {
           weapon.WeaponDamage += result.Damage;
           weapon.WeaponSpeed += result.Speed;
           weapon.WeaponRange += result.Range;
       }

       return result.Currency;
   }
   ```

### C. UpgradesController
Open [UpgradesController.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Controllers/UpgradesController.cs):
1. Add a case in `SpawnUpgrades()`:
   ```csharp
   case UpgradesCategoryType.YourNewWeapon:
       _button.onClick.AddListener(BuyYourNewWeaponUpgrade);
       break;
   ```
2. Create the callback method:
   ```csharp
   public void BuyYourNewWeaponUpgrade()
   {
       if (!_canChoose) return;
       if (_upgradesManager == null) return;

       int newSeed = _upgradesManager.UpgradeYourNewWeapon(_seedCollected.Value);
       if (newSeed != _seedCollected.Value) 
       {
           _seedCollected.Value = newSeed;
           _canChoose = false;
           TurnOffCanvasGroup();
       }
   }
   ```

### D. Attach Ability Component to Player Prefab
1. Open the [Player.prefab](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Prefabs/Player/Player.prefab).
2. Attach the `YourNewWeaponAbilityState` component to the `SpecialAbilityTest` child object (or player body).
3. Assign the fields in the inspector:
   - **Weapon Config**: Reference your `YourNewWeaponConfig.asset`.
   - **Target**: Reference the main Player Transform.
   - Configure ability multipliers, duration, and cooldown values.

---

## Step 7: UI Integration

Finally, add the weapon selection button in [WeaponPicker.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/WeaponPicker.cs):

1. Add a field for the picker button:
   ```csharp
   [SerializeField] private Button _yourNewWeaponButton;
   ```
2. Implement the pick callback method:
   ```csharp
   public void OnPickedYourNewWeapon()
   {
       Events_Weapons.OnChosenWeapon?.Invoke("YourNewWeaponConfig"); // Invokes loading of addressable config
       Tween.PunchScale(_yourNewWeaponButton.transform, new Vector3(-0.2f, -0.2f, 0), _buttonAnimationDuration, useUnscaledTime: true);
       _countIndex++;
   }
   ```
3. Update `TurnOnCanvasGroup()` and `TurnOffCanvasGroup()` to include `_yourNewWeaponButton` in the staggered scaling animations.
4. Set up the button in the UI hierarchy, assign the Unity click event to call `OnPickedYourNewWeapon()`, and drag the button reference into the script's inspector slot.
