using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Shared.Events
{
    public static class Events_VFX
    {
        public static Action<AssetReferenceGameObject, Vector3, Quaternion, float> SpawnVFXEvent { get; set; }
    }
}
