using Shared.Events;
using UnityEngine;

using Shared.Events;
using UnityEngine;

namespace Gameplay.Powerups
{
    public class PowerUpCollector : MonoBehaviour
    {
        [SerializeField] private AudioClip _audioClip;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PowerUp powerUp))
            {
                if (!powerUp.gameObject.activeSelf)
                {
                    return;
                }

                AudioClip clipToPlay = (powerUp.PowerUpConfig != null && powerUp.PowerUpConfig.PowerUpSound != null) 
                    ? powerUp.PowerUpConfig.PowerUpSound 
                    : _audioClip;

                if (clipToPlay != null)
                {
                    Events_Sound.PlaySound?.Invoke(clipToPlay);
                }

                powerUp.Collect();
            }
            else
            {
                // Fallback check if component is in parent
                PowerUp powerUpInParent = other.GetComponentInParent<PowerUp>();
                if (powerUpInParent != null)
                {
                    if (powerUpInParent.gameObject.activeSelf)
                    {
                        powerUpInParent.Collect();
                    }
                }
            }
        }
    }
}
