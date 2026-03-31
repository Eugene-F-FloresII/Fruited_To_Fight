using Controllers;
using Obvious.Soap.Example;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Enemy Configuration", fileName = "Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        public EnemyController EnemyPrefab;
        public int EnemyID;
        public int EnemyAmountToPool;
        public float EnemyHealth;
        public float EnemyDamage;
        public float EnemyMoveSpeed;
        public float EnemyAtkSpeed;
    }

}
