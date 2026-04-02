using Unity.Cinemachine;
using UnityEngine;
using Shared.Events;

namespace Gameplay
{
    public class ScreenShaker : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        private void OnEnable()  => Events_Character.OnShakeRequested += HandleShake;
        private void OnDisable() => Events_Character.OnShakeRequested -= HandleShake;

        private void HandleShake(float force) => _impulseSource.GenerateImpulse(force);
    }
}

