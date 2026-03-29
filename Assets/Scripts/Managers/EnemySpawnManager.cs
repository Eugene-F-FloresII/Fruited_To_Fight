using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private EnemyController _enemyPrefab;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Camera _camera;

        private void Start()
        {
            for (int i = 0; i < 20; i++)
            {
                _enemyPrefab = Instantiate(_enemyPrefab, GetEdgeSpawnPosition(), Quaternion.identity);
                _enemyPrefab.InitializePlayer(_playerController);
            }
            
            
        }

        Vector2 GetEdgeSpawnPosition() {
            _camera = Camera.main;
            float height = _camera.orthographicSize;
            float width = height * _camera.aspect;
            float margin = 2f;

            Vector3 camPos = _camera.transform.position;
            int side = Random.Range(0, 4);

            return side switch {
                0 => new Vector2(Random.Range(camPos.x - width - margin, camPos.x + width + margin), camPos.y + height + margin), // top
                1 => new Vector2(camPos.x + width + margin, Random.Range(camPos.y - height - margin, camPos.y + height + margin)), // right
                2 => new Vector2(Random.Range(camPos.x - width - margin, camPos.x + width + margin), camPos.y - height - margin), // bottom
                _ => new Vector2(camPos.x - width - margin, Random.Range(camPos.y - height - margin, camPos.y + height + margin)), // left
            };
        }
    }

}
