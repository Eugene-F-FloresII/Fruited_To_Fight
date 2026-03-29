using Obvious.Soap.Example;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Enemy Configuration", fileName = "Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        public int EnemyID;
        public float EnemyHealth;
        public float EnemyDamage;
        public float EnemyMoveSpeed;
        public float EnemyAtkSpeed;
    }

}
