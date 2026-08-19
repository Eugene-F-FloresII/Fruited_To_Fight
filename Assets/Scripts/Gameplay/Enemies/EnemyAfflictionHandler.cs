using System;
using System.Threading;
using Collection;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

namespace Gameplay.Enemies
{
    public class EnemyAfflictionHandler : MonoBehaviour
    {
        private EnemyMovement _enemyMovement;
        private EnemyVisuals _enemyVisuals;
        private CancellationTokenSource _freezeCts;

        private void Awake()
        {
            _enemyMovement = GetComponent<EnemyMovement>();
            _enemyVisuals = GetComponent<EnemyVisuals>();
        }

        private void OnDisable()
        {
            if (_freezeCts != null)
            {
                _freezeCts.Cancel();
                _freezeCts.Dispose();
                _freezeCts = null;
            }
        }

        public void ApplyAffliction(AfflictionConfig config, Controllers.EnemyController controller)
        {
            if (config == null) return;

            var existingAfflictions = GetComponents<AfflictionState>();
            foreach (var affliction in existingAfflictions)
            {
                if (affliction.AfflictionType == config.Type)
                {
                    affliction.Refresh(config);
                    return;
                }
            }

            AfflictionState newState = null;
            switch (config.Type)
            {
                case Shared.Enums.AfflictionType.Burn:
                    newState = gameObject.AddComponent<BurnState>();
                    break;
                case Shared.Enums.AfflictionType.Ice:
                    newState = gameObject.AddComponent<IceState>();
                    break;
                case Shared.Enums.AfflictionType.Weakness:
                    newState = gameObject.AddComponent<WeaknessState>();
                    break;
                case Shared.Enums.AfflictionType.Lightning:
                    newState = gameObject.AddComponent<LightningState>();
                    newState.InitializeVFXPrefab(config.VFXPrefab);
                    break;
            }

            if (newState != null)
            {
                newState.Initialize(controller, config);
            }
        }

        public async UniTaskVoid FreezeAsync(float duration)
        {
            if (_freezeCts != null)
            {
                _freezeCts.Cancel();
                _freezeCts.Dispose();
            }
            _freezeCts = new CancellationTokenSource();
            var token = _freezeCts.Token;

            try
            {
                if (_enemyMovement != null) _enemyMovement.SetFrozen(true);
                if (_enemyVisuals != null) _enemyVisuals.SetAnimationSpeed(0f);
                
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    if (_enemyMovement != null) _enemyMovement.SetFrozen(false);
                    if (_enemyVisuals != null) _enemyVisuals.SetAnimationSpeed(1f);
                }
            }
        }
    }
}
