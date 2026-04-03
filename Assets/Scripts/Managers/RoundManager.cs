using System;
using System.Collections.Generic;
using Collection;
using Controllers;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Managers
{
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<EnemyConfig> _enemyConfigReference;
        
        [SerializeField] private List<EnemyController> _enemiesInRound;
        
        private EnemyConfig _enemyConfig;
        private EnemySpawnManager _enemySpawnManager;
        
        private int _numberOfEnemiesSpawned;
        private bool _roundStarted;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            _enemySpawnManager = ServiceLocator.Get<EnemySpawnManager>();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<RoundManager>();
        }

        private void Update()
        {
            if (_roundStarted)
            {
                if (_numberOfEnemiesSpawned <= 0)
                {
                    InitiateRoundEnds();
                }
            }
        }

        private void InitiateRoundStarts()
        {
            _numberOfEnemiesSpawned += _enemySpawnManager.GetEnemiesAmount();
            _enemySpawnManager.SpawnEnemies();
            
            StartRound().Forget();
        }

        private void InitiateRoundEnds()
        {
            _roundStarted = false;
        }

        private async UniTaskVoid StartRound()
        {
            _roundStarted  = true;
        }
    }

}
