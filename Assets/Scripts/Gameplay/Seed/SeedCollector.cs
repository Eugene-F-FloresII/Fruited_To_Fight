using Obvious.Soap;
using Shared.Events;
using UnityEngine;

namespace Gameplay.Seed
{
    public class SeedCollector : MonoBehaviour
    {
        [SerializeField] private IntVariable _seedCollected;
        [SerializeField] private AudioClip _audioClip;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Seed seed))
            {
                if (!seed.gameObject.activeSelf)
                {
                    return;
                }

                _seedCollected.Value++;
                
                Events_Sound.PlaySound?.Invoke(_audioClip);
                
                seed.gameObject.SetActive(false);
            }
        }
    }

}
