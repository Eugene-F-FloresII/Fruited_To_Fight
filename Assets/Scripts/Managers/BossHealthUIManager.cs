using System;
using Collection;
using UnityEngine;

namespace Managers
{
    public class BossHealthUIManager : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<BossHealthUIManager>();
        }
    }

}
