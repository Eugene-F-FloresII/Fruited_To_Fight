using Controllers;
using UnityEngine;

namespace Gameplay.Enemies
{
    public class EnemyMovement : MonoBehaviour
    {
        private Rigidbody2D _enemyRb;
        private PlayerController _playerController;
        private EnemyVisuals _enemyVisuals;

        private void Awake()
        {
            _enemyRb = GetComponent<Rigidbody2D>();
            _enemyVisuals = GetComponent<EnemyVisuals>();
        }

        public void Initialize(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void MoveTowardsPlayer(float speed)
        {
            if (_playerController == null) return;

            float playerPosX = _playerController.transform.position.x;
            float playerPosY = _playerController.transform.position.y;
            
            if (_enemyVisuals != null)
            {
                _enemyVisuals.SetAnimationVelocity(playerPosX, playerPosY);
            }
            
            transform.position = Vector2.MoveTowards(transform.position, _playerController.transform.position, speed * Time.fixedDeltaTime);
        }

        public void ApplyImpulse(Vector2 force)
        {
            if (_enemyRb != null)
            {
                _enemyRb.linearVelocity = Vector2.zero;
                _enemyRb.AddForce(force, ForceMode2D.Impulse);
            }
        }

        public void ResetVelocity()
        {
            if (_enemyRb != null)
            {
                _enemyRb.linearVelocity = Vector2.zero;
            }
        }
    }
}
