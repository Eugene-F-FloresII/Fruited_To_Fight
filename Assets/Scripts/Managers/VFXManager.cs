using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class VFXManager : MonoBehaviour
    {
        private Dictionary<string, ObjectPool<GameObject>> _vfxPools = new Dictionary<string, ObjectPool<GameObject>>();
        private Dictionary<string, AsyncOperationHandle<GameObject>> _loadedHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();

        private void OnEnable()
        {
            Events_VFX.SpawnVFXEvent += OnSpawnVFX;
        }

        private void OnDisable()
        {
            Events_VFX.SpawnVFXEvent -= OnSpawnVFX;
        }

        private void OnDestroy()
        {
            foreach (var handle in _loadedHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadedHandles.Clear();
            _vfxPools.Clear();
        }

        private void OnSpawnVFX(AssetReferenceGameObject assetRef, Vector3 position, Quaternion rotation, float duration)
        {
            if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return;
            SpawnVFXAsyncOPAddress(assetRef, position, rotation, duration).Forget();
        }

        private async UniTaskVoid SpawnVFXAsyncOPAddress(AssetReferenceGameObject assetRef, Vector3 position, Quaternion rotation, float duration)
        {
            string key = assetRef.RuntimeKey.ToString();

            if (!_loadedHandles.ContainsKey(key))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(assetRef);
                _loadedHandles[key] = handle;
                await handle.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            else if (!_loadedHandles[key].IsDone)
            {
                await _loadedHandles[key].ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            if (!_vfxPools.ContainsKey(key))
            {
                GameObject prefab = _loadedHandles[key].Result;
                _vfxPools[key] = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab, transform),
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => Destroy(obj),
                    collectionCheck: false,
                    defaultCapacity: 20,
                    maxSize: 500
                );
            }

            GameObject vfxInstance = _vfxPools[key].Get();
            vfxInstance.transform.position = position;
            vfxInstance.transform.rotation = rotation;

            DespawnVFXAsyncOP(key, vfxInstance, duration).Forget();
        }

        private async UniTaskVoid DespawnVFXAsyncOP(string poolKey, GameObject instance, float delay)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (instance != null && _vfxPools.ContainsKey(poolKey))
            {
                _vfxPools[poolKey].Release(instance);
            }
        }
    }
}
