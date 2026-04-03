using System;

namespace Data
{
    [Serializable]
    public struct EnemyRuntimeStats
    {
        public float Health;
        public float Damage;
        public float MoveSpeed;
        public float AttackSpeed;
        public float KnockbackForce;

        public EnemyRuntimeStats(float health, float damage, float moveSpeed, float attackSpeed, float knockbackForce)
        {
            Health = health;
            Damage = damage;
            MoveSpeed = moveSpeed;
            AttackSpeed = attackSpeed;
            KnockbackForce = knockbackForce;
        }
    }
}
