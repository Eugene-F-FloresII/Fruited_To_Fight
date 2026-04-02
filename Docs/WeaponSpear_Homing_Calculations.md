# Weapon Spear Homing (Code-First, Math Explained)

This guide explains the homing calculations used by `WeaponSpear` in this project.

## 1) Target Direction

Code pattern:

```csharp
Vector2 desiredDirection = (Vector2)(target.transform.position - transform.position);
desiredDirection.Normalize();
```

Math meaning:

- `targetPos - projectilePos` gives a vector that points from spear to enemy.
- Normalizing turns it into unit length (magnitude = 1), so direction is separated from speed.

Formula:

`d = (p_target - p_projectile) / |p_target - p_projectile|`

Why this matters:

- You can use the same direction for velocity and rotation, then apply speed independently.

## 2) Nearest Target Selection

Code pattern:

```csharp
float distanceSqr = ((Vector2)enemy.transform.position - currentPosition).sqrMagnitude;
if (distanceSqr < closestDistanceSqr)
{
    closestDistanceSqr = distanceSqr;
    target = enemy;
}
```

Math meaning:

- Euclidean distance is `sqrt(dx^2 + dy^2)`.
- For comparison only, square root is unnecessary.

Formula:

`dist2 = dx^2 + dy^2`

Why use squared distance:

- Faster and stable for frequent checks in physics updates.
- Ordering is identical to actual distance because sqrt is monotonic.

## 3) Snap Steering vs Smooth Steering

### Snap (used after pierce retarget)

Code idea:

```csharp
nextDirection = desiredDirection;
```

- Immediately points to the selected target.
- Good when you want sharp retarget behavior on impact.

### Smooth Turn-Rate Steering (continuous homing)

Code pattern:

```csharp
float turnRadians = Mathf.Deg2Rad * homingTurnSpeedDegrees * deltaTime;
Vector2 nextDirection = Vector3.RotateTowards(currentDirection, desiredDirection, turnRadians, 0f);
```

Math meaning:

- You cap angular change each tick.
- Let `omega` be max angular speed (rad/s), `dt` be timestep.
- Max angle step this tick: `theta_max = omega * dt`.

In code:

- `homingTurnSpeedDegrees` is deg/s.
- `Mathf.Deg2Rad * homingTurnSpeedDegrees` converts to rad/s.
- Multiply by `deltaTime` to get allowed radians this frame.

Why this matters:

- Produces curved trajectories instead of instant snapping every frame.
- Gives a tunable feel: low value = wide arcs, high value = aggressive turning.

## 4) Velocity Update

Code pattern:

```csharp
_weaponRb.linearVelocity = nextDirection * _currentSpeed;
```

Math meaning:

- Velocity vector is direction multiplied by scalar speed.

Formula:

`v = d * s`

Where:

- `d` is unit direction.
- `s` is speed in units per second.

## 5) Rotation Update from Direction

Code pattern:

```csharp
float angle = Mathf.Atan2(nextDirection.y, nextDirection.x) * Mathf.Rad2Deg;
transform.rotation = Quaternion.Euler(0, 0, angle + projectileRotationOffset);
```

Math meaning:

- `atan2(y, x)` gives heading angle of vector `(x, y)`.
- Convert rad -> deg for Unity rotation APIs.
- Add sprite offset if your sprite forward axis is not +X.

In this project:

- `ProjectileRotationOffset = -90f` because spear art orientation requires correction.

## 6) Why Pooling Needed `OnTriggerStay2D`

When a pooled spear is re-enabled, enemies may already overlap the homing trigger.

- `OnTriggerEnter2D` may not fire immediately for pre-existing overlaps.
- `OnTriggerStay2D` guarantees overlapping enemies are tracked on subsequent physics ticks.

So pooling-safe target collection uses both callbacks.

## 7) Stability Guards You Should Always Keep

- Skip homing if target list is empty.
- Ignore null/inactive enemies.
- Ignore near-zero direction vectors before normalize.
- Keep list deduplicated.

These prevent jitter, invalid vectors, and stale pooled references.

## 8) Practical Tuning Cheat Sheet

- `homingTurnSpeedDegrees = 180` -> very soft turns.
- `homingTurnSpeedDegrees = 360` -> moderate turns.
- `homingTurnSpeedDegrees = 720` -> aggressive turns.
- `homingTurnSpeedDegrees = 1080+` -> almost snap-like.

Start around `540-720` and tune by feel.

## 9) Mapping to Current Code

- Target selection: `WeaponSpear.TryGetNearestTarget(...)`
- Steering: `WeaponSpear.SteerTowardsTarget(...)`
- Continuous homing loop: `WeaponSpear.FixedUpdate()` -> `UpdateContinuousHoming()`
- Pool-safe enemy tracking: `WeaponTriggerHoming.OnTriggerEnter2D/OnTriggerStay2D`
