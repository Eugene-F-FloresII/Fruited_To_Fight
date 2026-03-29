using System;
using Data;
using Obvious.Soap.Example;
using UnityEngine;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyConfig _enemyConfig;
        
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Animator _animator;
        
        private float _currentHealth;
        private float _currentSpeed;
        private void Awake()
        {
            _currentHealth = _enemyConfig.EnemyHealth;
            _currentSpeed = _enemyConfig.EnemyMoveSpeed;
        }

        private void FixedUpdate()
        {
            ChasePlayer();
        }

        public void InitializePlayer(PlayerController playerController) =>  _playerController = playerController; 

        private void ChasePlayer()
        {
            var Xpos = _playerController.transform.position.x;
            var Ypos = _playerController.transform.position.y;
            
            _animator.SetFloat("VelocityX", Xpos);
            _animator.SetFloat("VelocityY", Ypos);
            
            
            gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, _playerController.transform.position, _currentSpeed * Time.deltaTime);
        }
    }

}
