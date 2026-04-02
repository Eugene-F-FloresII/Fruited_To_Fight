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
                _seedCollected.Value++;
            }
        }
    }

}
