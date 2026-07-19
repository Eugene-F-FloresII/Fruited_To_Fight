using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Data.Upgrades;
using Managers;
using Shared.Events;
using PrimeTween;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Shared.Enums;
using Data;
using Collection;

namespace Controllers
{
    public class UpgradeAfflictionController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.5f;

        [Header("Affliction Settings")]
        [SerializeField] private int _maxButtons = 3;
        [SerializeField] private Transform _transform;

        [SerializeField] private List<UpgradeAfflictionData> _afflictionList = new List<UpgradeAfflictionData>();
        private UpgradesManager _upgradesManager;
        private CanvasGroup _canvasGroup;
        private bool _canChoose;
        private string _currentWeaponKey;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();

            _canChoose = true;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            Events_Upgrades.OnActivateUpgradeAfflictionPanel += TurnOnCanvasGroup;
            Events_Upgrades.OnRoundStarted += TurnOffCanvasGroup;
            Events_Game.OnGameRestarted += OnGameRestarted;
            Events_Game.OnGameExited += OnGameRestarted;
            Events_Weapons.OnChosenWeapon += OnWeaponChosen;
        }

        private void OnDisable()
        {
            Events_Upgrades.OnActivateUpgradeAfflictionPanel -= TurnOnCanvasGroup;
            Events_Upgrades.OnRoundStarted -= TurnOffCanvasGroup;
            Events_Game.OnGameRestarted -= OnGameRestarted;
            Events_Game.OnGameExited -= OnGameRestarted;
            Events_Weapons.OnChosenWeapon -= OnWeaponChosen;
        }

        private void OnWeaponChosen(string key)
        {
            _currentWeaponKey = key;
            _afflictionList.Clear();
            LoadAfflictions(key).Forget();
            
            Debug.Log(key);
        }

        private async UniTask LoadAfflictions(string label)
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<UpgradeAfflictionData>(label);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _afflictionList.Add(handle.Result);
                }
                else
                {
                    Debug.LogWarning($"No UpgradeAfflictionData found with label '{label}'");
                }
            }
            catch (UnityEngine.AddressableAssets.InvalidKeyException)
            {
                Debug.Log($"No UpgradeAfflictionData registered with label '{label}' (this is normal if the weapon has no affliction upgrades).");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Exception loading UpgradeAfflictionData with label '{label}': {ex.Message}");
            }
        }

        private void OnGameRestarted()
        {
            _canChoose = true;
            _currentWeaponKey = string.Empty;
            _afflictionList.Clear();
            TurnOffCanvasGroup();
        }

        private void ClearUpgrades()
        {
            if (_transform == null) return;
            foreach (Transform child in _transform)
            {
                Destroy(child.gameObject);
            }
        }

        private WeaponConfig GetTargetWeaponConfig()
        {
            Debug.Log("GetTargetWeaponConfig method");
            
            if (_upgradesManager == null) return null;
            
            if (_upgradesManager.FirstWeaponConfig != null)
            {
                return _upgradesManager.FirstWeaponConfig;
            }

            if (_upgradesManager.SecondWeaponConfig != null)
            {
                return _upgradesManager.SecondWeaponConfig;
            }

            return null;
        }

        private async UniTask SpawnAfflictions()
        {
            if (_afflictionList == null || _afflictionList.Count == 0) return;
            
            WeaponConfig targetWeapon = GetTargetWeaponConfig();
            Debug.Log(targetWeapon.WeaponClass);
            if (targetWeapon == null) return;

            ClearUpgrades();

            List<UpgradeAfflictionData> availableAfflictions = new List<UpgradeAfflictionData>();
            
            foreach (var affliction in _afflictionList)
            {
                if (affliction.WeaponClass == targetWeapon.WeaponClass)
                {
                    availableAfflictions.Add(affliction);
                }
            }

            int buttonsToSpawn = Mathf.Min(_maxButtons, availableAfflictions.Count);

            for (int i = 0; i < buttonsToSpawn; i++)
            {
                int randomIndex = Random.Range(0, availableAfflictions.Count);
                var affliction = availableAfflictions[randomIndex];
                availableAfflictions.RemoveAt(randomIndex);

                Button button = Instantiate(affliction.ButtonPrefab, _transform);
                button.onClick.AddListener(() => BuyAffliction(affliction, targetWeapon));

                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }

        public async void TurnOnCanvasGroup()
        {
            if (_canvasGroup == null) return;

            _canChoose = true;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            await Tween.Alpha(_canvasGroup, 1f, _animationDuration).ToUniTask(this);
            await SpawnAfflictions();
        }

        public async void TurnOffCanvasGroup()
        {
            if (_canvasGroup == null) return;

            await Tween.Alpha(_canvasGroup, 0f, _animationDuration).ToUniTask(this);

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                ClearUpgrades();
            }
        }

        private void BuyAffliction(UpgradeAfflictionData data, WeaponConfig weapon)
        {
            if (!_canChoose) return;
            if (weapon == null) return;

            weapon.AddAffliction(data.AfflictionKey).Forget();
            _canChoose = false;
            TurnOffCanvasGroup();
        }
    }
}
