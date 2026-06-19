# Projectile Weapon Affliction System Guide

This guide explains how **Elemental Afflictions** (Status Effects) work on projectile weapons and enemies in the **Fruited to Fight** project. It details the lifecycle of an affliction—from weapon hit detection to visual upgrades, and provides step-by-step instructions on how to add affliction support to a new weapon prefab in the Unity Editor.

---

## Table of Contents
1. [Affliction Architecture Overview](#1-affliction-architecture-overview)
2. [How to Set Up Afflictions on a Weapon Prefab (Unity Editor)](#2-how-to-set-up-afflictions-on-a-weapon-prefab-unity-editor)
3. [How Weapons Apply Afflictions](#3-how-weapons-apply-afflictions)
4. [Weapon Visual Feedback (Weapon & Projectile Visuals)](#4-weapon-visual-feedback)
5. [Enemy Status Effects (Affliction States)](#5-enemy-status-effects)
6. [Visual Feedback on Enemies](#6-visual-feedback-on-enemies)
7. [Design Notes & Guidelines](#7-design-notes--guidelines)

---

## 1. Affliction Architecture Overview

The affliction system uses a decoupled event-driven pattern:
- **Weapon Config**: [WeaponConfig](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Data/WeaponConfig.cs) stores a list of [AfflictionConfig](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Data/AfflictionConfig.cs) objects representing the weapon's active elemental effects.
- **Weapon hit triggers**: [ProjectileWeapon](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileWeapon.cs) passes the config to the hit enemy controller.
- **Dynamic MonoBehaviours on Enemies**: The enemy controller attaches a specialized state class (inheriting from [AfflictionState](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Collection/AfflictionState.cs)) to handle ticks, stacks, and timers dynamically.
- **Visual Controllers**: Decoupled scripts update the sprite animations and particle trails on both the weapon and the enemy.

---

## 2. How to Set Up Afflictions on a Weapon Prefab (Unity Editor)

To ensure a new projectile weapon displays visual effects (like flame trails or sprite color changes) when an affliction is purchased, follow these steps in the Unity Editor:

### A. Set Up the Projectile Prefab (Visual Effects & Trails)
The individual projectile prefab (e.g. `Tomahawk.prefab`) controls the particle systems and animator changes when flying.

1. Open your **Projectile Prefab** in Prefab Edit Mode.
2. Create your visual effect child GameObjects (e.g. `FireVFX`, `IceVFX`) under the projectile root.
3. Attach the [WeaponAffliction.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/WeaponAffliction.cs) component to the projectile.
4. Configure the fields on the `WeaponAffliction` component in the Inspector:
   - **Animator**: Drag the projectile's `Animator` component here.
   - **Weapon Afflictions**: This is a list mapping indices to elemental states. Set the size and drag the child VFX GameObjects:
     - `Index 0`: Default/None visual (optional)
     - `Index 1`: Fire/Burn VFX GameObject (e.g. `FireVFX`)
     - `Index 2`: Ice/Freeze VFX GameObject (e.g. `IceVFX`)
     - `Index 3`: Weakness VFX GameObject
   - **Default Trail**: Drag the default trail renderer or particle effect GameObject here (it will automatically turn off when an elemental trail becomes active).
5. Select the projectile script component (inheriting from `ProjectileWeapon`) and drag the `WeaponAffliction` component into the **Weapon Affliction** slot under the **Weapon Afflictions** header.
6. *(Optional)* In your weapon's Animator Controller, add animator layers mapped by name (e.g. create a layer named `"Fire Affliction"`) to overlay animations.

### B. Set Up the Spawner Prefab (Sprite Visual Changes)
The spawner prefab (e.g. `TomahawkSpawner.prefab`) is held by the player and updates its sprite sheet animation depending on the affliction.

1. Open your **Spawner Prefab** in Prefab Edit Mode.
2. Attach the [WeaponAfflictionSprite.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/WeaponAfflictionSprite.cs) component to the GameObject that contains the `SpriteRenderer`.
3. Configure the fields on the `WeaponAfflictionSprite` component in the Inspector:
   - **Sprite Renderer**: Drag the weapon's `SpriteRenderer` component here.
   - **Affliction Sprites**: Add mappings for each affliction type:
     - Click **+** to add a mapping.
     - Set **Affliction Type** (e.g. `Burn`, `Ice`).
     - Expand the **Sprites** list and assign the animated sprites representing that element.
   - **Fps**: Frame rate for the sprite animation loop (e.g. `12` frames per second).
4. Select your spawner script component (inheriting from `ProjectileSpawner`) and drag the `WeaponAfflictionSprite` component into the **Weapon Affliction Sprite** field in the Inspector.
5. Save your prefab.

---

## 3. How Weapons Apply Afflictions

### A. Dynamic Acquisition
When an affliction upgrade is bought via the [UpgradeAfflictionController](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Controllers/UpgradeAfflictionController.cs), it loads the corresponding `AfflictionConfig` from Addressables and calls:
```csharp
weapon.AddAffliction(data.AfflictionKey).Forget();
```
This adds the `AfflictionConfig` to the `WeaponConfig.Afflictions` list and fires the `OnAfflictionsChanged` action.

### B. Trigger Collision
On collision with an enemy, [ProjectileWeapon.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileWeapon.cs) loops through all active afflictions on the weapon's config and applies them:
```csharp
protected virtual void OnTriggerEnter2D(Collider2D other)
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
        // ... pierce and despawn logic
    }
}
```

---

## 4. Weapon Visual Feedback

Both the [ProjectileSpawner](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileSpawner.cs) and active [ProjectileWeapon](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/ProjectileWeapon.cs) instances subscribe to `OnAfflictionsChanged`. Upon triggering, they call `RefreshAfflictionVisuals()`:

```csharp
AfflictionType type = _weaponConfig.Afflictions.Count > 0 
    ? _weaponConfig.Afflictions[0].Type 
    : AfflictionType.None;
```

This updates two components:

1. **[WeaponAfflictionSprite.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/WeaponAfflictionSprite.cs)**:
   - Evaluates the active `AfflictionType`.
   - Selects the mapped list of frames (`List<Sprite>`).
   - Plays a UniTask-based frame loop to animate the weapon/projectile sprite (e.g., rendering a flaming axe instead of a normal steel axe).
2. **[WeaponAffliction.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Weapons/WeaponAffliction.cs)**:
   - Toggles elemental VFX GameObject layers (e.g., activating fire particle trails and deactivating default ones).
   - Adjusts the **Animator** layers. It raises the weight of the corresponding layer (e.g. "Fire Affliction") to `1.0f` to overlay elemental swinging animations.

---

## 5. Enemy Status Effects

When `EnemyController.ApplyAffliction` is called, it checks if a MonoBehaviour component for that affliction is already present:
- **If present**: Refreshes duration and increments status stacks (up to `Config.MaxStacks`).
- **If absent**: Dynamically attaches the MonoBehaviour corresponding to the affliction type using `gameObject.AddComponent<T>()`.

Each status effect behaves as follows:

| Affliction Script | Enum Type | Description |
| :--- | :--- | :--- |
| **[BurnState](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Collection/BurnState.cs)** | `Burn` | Periodically deals damage equal to `Config.Power` every `1` second. |
| **[IceState](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Collection/IceState.cs)** | `Ice` | Accumulates stacks. Upon reaching `MaxStacks`, triggers `EnemyController.Freeze(duration)` (freezing animator speed to `0` and pausing locomotion). |
| **[WeaknessState](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Collection/WeaknessState.cs)** | `Weakness` | Deals instant damage equal to 10% of the enemy's current health. |
| **[LightningState](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Collection/LightningState.cs)** | `Lightning` | Accumulates stacks. Upon reaching `MaxStacks`, instantiates a lightning strike VFX, delays, and deals `Config.Power * 4` damage to all enemies within `Config.ExplosionRadius`. |

---

## 6. Visual Feedback on Enemies

Every enemy prefab includes the **[EnemyAffliction.cs](file:///d:/UnityProjects/Fruited_To_Fight/Assets/Scripts/Gameplay/Enemies/EnemyAffliction.cs)** child component:
1. When an affliction is initialized, it calls `VisualController.ToggleVisual(type, true)`.
2. This activates the corresponding VFX GameObject child on the enemy (e.g., small burning flames or ice blocks around the enemy sprite).
3. The component starts a UniTask-based duration timer.
4. When the timer expires (or the affliction component on the enemy is destroyed), it calls `VisualController.ToggleVisual(type, false)` to hide the VFX.

---

## 7. Design Notes & Guidelines

> [!IMPORTANT]
> **Single Visual vs. Multiple Effects**: While `WeaponConfig` holds a `List<AfflictionConfig>` allowing multiple effects to be applied simultaneously behind the scenes, the weapon's visual controllers (`WeaponAffliction` and `WeaponAfflictionSprite`) only read **the first element** (`Afflictions[0]`) to update the weapon's appearance.

- **Stack Caps**: If adding custom afflictions, override `OnStackAdded` to check if `CurrentStacks` equals `Config.MaxStacks` to trigger burst effects (like ice freezes or lightning strikes).
- **VFX cleanup**: Dynamic visual prefabs instantiated in affliction states (e.g., lightning bolts) must be manually garbage-collected or destroyed after completion (e.g., `Destroy(VFXGameObject, delay)`).
