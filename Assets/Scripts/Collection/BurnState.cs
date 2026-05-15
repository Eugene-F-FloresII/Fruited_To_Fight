using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Controllers;
using Data;
using UnityEngine;

namespace Collection
{
    public class BurnState : AfflictionState
    {
        private CancellationTokenSource _burnCts;

        public override void Initialize(EnemyController enemy, AfflictionConfig config)
        {
            base.Initialize(enemy, config);
            StartBurning().Forget();
        }

        private async UniTaskVoid StartBurning()
        {
            _burnCts = new CancellationTokenSource();
            var token = _burnCts.Token;

            try
            {
                while (RemainingDuration > 0 && !token.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                    if (Enemy != null && Enemy.gameObject.activeInHierarchy)
                    {
                        Enemy.TakeDamage(Config.Power);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnDestroy()
        {
            if (_burnCts != null)
            {
                _burnCts.Cancel();
                _burnCts.Dispose();
            }
        }
    }
}
