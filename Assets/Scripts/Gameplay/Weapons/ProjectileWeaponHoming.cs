using System.Collections.Generic;
using Controllers;
using Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class ProjectileWeaponHoming : MonoBehaviour
    {
        [SerializeField] private WeaponConfig _weaponConfig;
        
        public List<EnemyController> Enemies;
        
        private CircleCollider2D _collider;
        
        private void Awake()
        {
           UpdateTriggerHoming();
        }

        private void Update()
        {
            Enemies.RemoveAll(e => e == null);
        }

        private void OnEnable()
        {
            Enemies ??= new List<EnemyController>();
            
            UpdateTriggerHoming();
        }

        private void OnDisable()
        {
            Enemies.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            {
                Enemies.Add(enemy);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            {
                Enemies.Remove(enemy);
            }
        }

        private void UpdateTriggerHoming()
        {
            _collider = GetComponent<CircleCollider2D>();
            _collider.radius = _weaponConfig.WeaponRange;
        }
        
        
    }

}
