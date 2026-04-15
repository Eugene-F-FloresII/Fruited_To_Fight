using System;

namespace Data
{
    [Serializable]
    public struct EnemyStatMultipliers
    {
        public float HealthMultiplier;
        public float DamageMultiplier;
        public float MoveSpeedMultiplier;
        public float AttackSpeedMultiplier;
        public float KnockbackMultiplier;

        public EnemyStatMultipliers(float health, float damage, float moveSpeed, float attackSpeed, float knockback)
        {
            HealthMultiplier = health;
            DamageMultiplier = damage;
            MoveSpeedMultiplier = moveSpeed;
            AttackSpeedMultiplier = attackSpeed;
            KnockbackMultiplier = knockback;
        }
    }
}
