using Obvious.Soap;
using UnityEngine;

namespace Gameplay.Seed
{
    public class SeedCollector : MonoBehaviour
    {
        [SerializeField] private IntVariable _seedCollected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Seed seed))
            {
                if (!seed.gameObject.activeSelf)
                {
                    return;
                }

                _seedCollected.Value++;
                seed.gameObject.SetActive(false);
            }
        }
    }

}
