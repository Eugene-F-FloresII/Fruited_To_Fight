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
        public float EnemyKnockbackForce;

        [Header("Sound Settings")]
        public AudioClip DeathSFX;
        [Range(0f, 1f)] public float DeathSFXVolume = 1f;
        public AudioClip HitSFX;
        [Range(0f, 1f)] public float HitSFXVolume = 1f;
        
        [Header("VFX")]
        public UnityEngine.AddressableAssets.AssetReferenceGameObject DeathVFX;
        public float DeathVFXDuration = 1f;
    }

}
