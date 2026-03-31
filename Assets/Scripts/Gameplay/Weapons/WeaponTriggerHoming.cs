using System;
using System.Collections.Generic;
using Controllers;
using Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponTriggerHoming : MonoBehaviour
    {
        [SerializeField] private WeaponConfig _weaponConfig;
        
        public List<EnemyController> Enemies;
        
        private CircleCollider2D _collider;
        
        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _collider.radius = _weaponConfig.WeaponRange;
        }

        private void Update()
        {
            Enemies.RemoveAll(e => e == null);
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
        
        
    }

}
