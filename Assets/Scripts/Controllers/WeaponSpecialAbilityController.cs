using System;
using System.Collections.Generic;
using System.Threading;
using Collection;
using Cysharp.Threading.Tasks;
using Data;
using Shared.Enums;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Controllers
{
    /// <summary>
    /// Manages the loading, instantiation, and execution of weapon special abilities dynamically.
    /// </summary>
    public class WeaponSpecialAbilityController : MonoBehaviour
    {
        /// <summary>
        /// Pairs a weapon class with its corresponding addressable ability prefab reference.
        /// </summary>
        [System.Serializable]
        public struct WeaponAbilityMapping
        {
            public WeaponClass WeaponClass;
            public AssetReferenceGameObject AbilityPrefabReference;
        }

        [Header("Ability Mappings")]
        [SerializeField] private List<WeaponAbilityMapping> _weaponAbilitiesList;

        [Header("Injection Settings")]
        [SerializeField] private Transform _targetTransform;

        private readonly Dictionary<WeaponClass, AssetReferenceGameObject> _abilityPrefabsDict = new Dictionary<WeaponClass, AssetReferenceGameObject>();
        private readonly Dictionary<WeaponClass, WeaponAbilityState> _instantiatedAbilitiesDict = new Dictionary<WeaponClass, WeaponAbilityState>();
        private readonly List<AsyncOperationHandle<GameObject>> _loadingHandlesList = new List<AsyncOperationHandle<GameObject>>();

        private void Awake()
        {
            InitializeMappings();
        }

        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += OnWeaponChosen;
            Events_Weapons.OnUsedSpecialAbility += OnUsedSpecialAbility;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= OnWeaponChosen;
            Events_Weapons.OnUsedSpecialAbility -= OnUsedSpecialAbility;
        }

        private void OnDestroy()
        {
            CleanupAssetsAddress();
        }

        /// <summary>
        /// Populates the dictionary from the serialized mapping list.
        /// </summary>
        private void InitializeMappings()
        {
            foreach (var mapping in _weaponAbilitiesList)
            {
                if (mapping.AbilityPrefabReference != null && !_abilityPrefabsDict.ContainsKey(mapping.WeaponClass))
                {
                    _abilityPrefabsDict.Add(mapping.WeaponClass, mapping.AbilityPrefabReference);
                }
            }
        }

        /// <summary>
        /// Handles the event when a weapon is chosen. Preloads and instantiates the ability.
        /// </summary>
        /// <param name="weaponConfigKey">The Addressables key/label of the WeaponConfig.</param>
        private void OnWeaponChosen(string weaponConfigKey)
        {
            PrepareAbilityAsyncAddress(weaponConfigKey).Forget();
        }

        /// <summary>
        /// Asynchronously loads the weapon config and instantiates its corresponding special ability prefab.
        /// </summary>
        /// <param name="weaponConfigKey">The Addressables key/label of the WeaponConfig.</param>
        private async UniTask PrepareAbilityAsyncAddress(string weaponConfigKey)
        {
            CancellationToken token = this.GetCancellationTokenOnDestroy();

            try
            {
                var configHandle = Addressables.LoadAssetAsync<WeaponConfig>(weaponConfigKey);
                await configHandle.ToUniTask(cancellationToken: token);

                if (configHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[WeaponSpecialAbilityController] Failed to load WeaponConfig: {weaponConfigKey}");
                    return;
                }

                WeaponConfig weaponConfig = configHandle.Result;
                WeaponClass weaponClass = weaponConfig.WeaponClass;

                if (!_abilityPrefabsDict.TryGetValue(weaponClass, out var assetRef))
                {
                    // no mapping found for this weapon class, release handle and return
                    Addressables.Release(configHandle);
                    return;
                }

                if (_instantiatedAbilitiesDict.ContainsKey(weaponClass))
                {
                    // already prepared, release handle and return
                    Addressables.Release(configHandle);
                    return;
                }

                // instantiate the ability prefab under this controller's transform
                var spawnHandle = Addressables.InstantiateAsync(assetRef, transform);
                _loadingHandlesList.Add(spawnHandle);

                await spawnHandle.ToUniTask(cancellationToken: token);

                if (spawnHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject spawnedObj = spawnHandle.Result;
                    if (spawnedObj.TryGetComponent(out WeaponAbilityState abilityState))
                    {
                        abilityState.Initialize(_targetTransform);
                        _instantiatedAbilitiesDict.Add(weaponClass, abilityState);
                        Debug.Log($"[WeaponSpecialAbilityController] Successfully prepared special ability for {weaponClass}");
                    }
                    else
                    {
                        Debug.LogError($"[WeaponSpecialAbilityController] Instantiated prefab for {weaponClass} does not have a WeaponAbilityState component.");
                        Addressables.Release(spawnedObj);
                    }
                }
                else
                {
                    Debug.LogError($"[WeaponSpecialAbilityController] Failed to instantiate special ability prefab for {weaponClass}");
                }

                // release the temporary weaponConfig asset reference
                Addressables.Release(configHandle);
            }
            catch (OperationCanceledException)
            {
                // silently consume cancellation on game exit/restart
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Handles the event when a special ability is used, invoking the cached ability.
        /// </summary>
        /// <param name="weaponClass">The WeaponClass whose ability is being invoked.</param>
        private void OnUsedSpecialAbility(WeaponClass weaponClass)
        {
            if (_instantiatedAbilitiesDict.TryGetValue(weaponClass, out var abilityState))
            {
                abilityState.UseWeaponAbility();
            }
            else
            {
                Debug.LogWarning($"[WeaponSpecialAbilityController] Special ability for {weaponClass} is not loaded/prepared yet!");
            }
        }

        /// <summary>
        /// Releases all loaded/instantiated Addressable resources.
        /// </summary>
        private void CleanupAssetsAddress()
        {
            foreach (var handle in _loadingHandlesList)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadingHandlesList.Clear();
            _instantiatedAbilitiesDict.Clear();
            _abilityPrefabsDict.Clear();
        }
    }
}
