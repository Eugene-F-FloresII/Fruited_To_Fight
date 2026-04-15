using Data;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Gameplay.Weapons
{
    public class WeaponLoader : MonoBehaviour
    {
        [SerializeField] private Transform _rightHandSpawner;
        [SerializeField] private Transform _leftHandSpawner;

        private bool _isWeaponMelee;
        private bool _rightHandOccupied;
        private bool _leftHandOccupied;
        private WeaponConfig _weaponConfig;
        private GameObject _weaponPrefab;
        private GameObject _weapon;
        
        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += InitializeWeapon;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= InitializeWeapon;
        }

        private void InitializeWeapon(string weapon)
        {
           PreparingWeapon(weapon).Forget();
        }

        private async UniTask PreparedWeapon(WeaponConfig weaponConfig)
        {
            GameObject weaponInstance = await WeaponSpawner(weaponConfig);
    
            if (weaponInstance == null) return;

            if (!_leftHandOccupied)
            {
                Instantiate(weaponInstance, _leftHandSpawner.position, _leftHandSpawner.rotation, _leftHandSpawner);
                _leftHandOccupied = true;
                Events_Weapons.OnSpawnedWeapon?.Invoke(weaponInstance);
            }
            else if (!_rightHandOccupied)
            {
                Instantiate(weaponInstance, _rightHandSpawner.position, _rightHandSpawner.rotation, _rightHandSpawner);
                _rightHandOccupied = true;
                Events_Weapons.OnSpawnedWeapon?.Invoke(weaponInstance);
            }
            else
            {
                Debug.LogWarning("Both hands occupied, cannot equip weapon.");
            }
        }

        private async UniTask<GameObject> WeaponSpawner(WeaponConfig weaponConfig)
        {
            if (!weaponConfig.WeaponSpawner.RuntimeKeyIsValid())
            {
                Debug.LogError($"Invalid AssetReference for {weaponConfig.WeaponName}");
                return null;
            }

            // Check if the asset is already loaded
            if (weaponConfig.WeaponSpawner.Asset != null)
            {
                return weaponConfig.WeaponSpawner.Asset as GameObject;
            }

            // Check if there is an ongoing load operation
            if (weaponConfig.WeaponSpawner.OperationHandle.IsValid())
            {
                var existingHandle = weaponConfig.WeaponSpawner.OperationHandle.Convert<GameObject>();
                await existingHandle.Task;
                return existingHandle.Result;
            }

            // Start a new load if not already loading or loaded
            var handle = weaponConfig.WeaponSpawner.LoadAssetAsync<GameObject>();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Debug.LogError($"Failed to load WeaponSpawner for {weaponConfig.WeaponName}");
            return null;
        }

        async UniTask PreparingWeapon(string weapon)
        {
            var handle = Addressables.LoadAssetAsync<WeaponConfig>(weapon); // label
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _weaponConfig =  handle.Result;
                PreparedWeapon(_weaponConfig).Forget();
            }
            else
            {
                Debug.LogError("Failed to load WeaponConfig with label 'Spear'");
            }
        }
        
    }
}
