using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Obvious.Soap;
using Data;
using Collection;
using Managers;

namespace Controllers
{
    public class DevConsoleController : MonoBehaviour
    {
        private static DevConsoleController _instance;

        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private GameObject _buttonPrefab;

        [Header("SOAP References")]
        [SerializeField] private IntVariable _seedsCollected;
        [SerializeField] private CurrencyConfig _currencyConfig;
        [SerializeField] private List<IntVariable> _permaUpgradeVariables = new List<IntVariable>();

        private bool _isOpen;

        private struct DevConsoleCommand
        {
            public string Name;
            public string ActionText;
            public System.Action Callback;

            public DevConsoleCommand(string name, string actionText, System.Action callback)
            {
                Name = name;
                ActionText = actionText;
                Callback = callback;
            }
        }

        private List<DevConsoleCommand> _commands = new List<DevConsoleCommand>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            if (_contentContainer == null)
            {
                _contentContainer = transform.Find("Canvas/DevConsolePanel/ScrollMenu/Content");
            }

            SetConsoleActive(false);
            RegisterAllCommands();
            PopulateConsoleUI();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                ToggleConsole();
            }
        }

        private void ToggleConsole()
        {
            SetConsoleActive(!_isOpen);
        }

        private void SetConsoleActive(bool active)
        {
            _isOpen = active;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = active ? 1f : 0f;
                _canvasGroup.blocksRaycasts = active;
                _canvasGroup.interactable = active;
            }
        }

        private void RegisterAllCommands()
        {
            _commands.Clear();
            _commands.Add(new DevConsoleCommand("Add +100 Seeds", "Add", () => AddSeeds(100)));
            _commands.Add(new DevConsoleCommand("Add +1000 Seeds", "Add", () => AddSeeds(1000)));
            _commands.Add(new DevConsoleCommand("God Mode (Invincible)", "Toggle", ToggleGodMode));
            _commands.Add(new DevConsoleCommand("Kill All Enemies", "Kill", KillAllEnemies));
            _commands.Add(new DevConsoleCommand("Skip Round / Next Round", "Skip", SkipRound));
            _commands.Add(new DevConsoleCommand("Heal Player to Full", "Heal", HealPlayerToFull));
            _commands.Add(new DevConsoleCommand("Reset Gameplay Upgrades", "Reset", ResetGameplayUpgrades));
            _commands.Add(new DevConsoleCommand("Set Skeletal Leafs to 9999", "Cheat", SetSkeletalLeafCurrency));
            _commands.Add(new DevConsoleCommand("Reset Permanent Upgrades", "Reset", ResetPermanentUpgrades));
        }

        private void PopulateConsoleUI()
        {
            if (_contentContainer == null || _buttonPrefab == null)
            {
                Debug.LogWarning("[DevConsole] Container or Button Prefab reference is missing!");
                return;
            }

            // Clean up any existing children
            foreach (Transform child in _contentContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var cmd in _commands)
            {
                GameObject btnObj = Instantiate(_buttonPrefab, _contentContainer);
                btnObj.name = $"Btn_{cmd.Name.Replace(" ", "_")}";

                // Set Description Text (Txt_DevConsoleText)
                var labelText = btnObj.transform.Find("Txt_DevConsoleText")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (labelText != null)
                {
                    labelText.text = cmd.Name;
                }

                // Set Action Text (Btn_DevConsoleCommand/Text (TMP))
                var actionText = btnObj.transform.Find("Btn_DevConsoleCommand/Text (TMP)")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (actionText != null)
                {
                    actionText.text = cmd.ActionText;
                }

                // Setup Click Listener
                var button = btnObj.transform.Find("Btn_DevConsoleCommand")?.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    var callback = cmd.Callback;
                    button.onClick.AddListener(() =>
                    {
                        callback?.Invoke();
                    });
                }
            }
        }

        // --- DEV COMMAND HANDLERS ---

        private void AddSeeds(int amount)
        {
            if (_seedsCollected != null)
            {
                _seedsCollected.Value += amount;
                Debug.Log($"[DevConsole] Added {amount} seeds. Total: {_seedsCollected.Value}");
            }
            else
            {
                Debug.LogWarning("[DevConsole] SeedsCollected IntVariable reference not assigned.");
            }
        }

        private void ToggleGodMode()
        {
            var player = ServiceLocator.TryGet<PlayerController>();
            if (player != null && player.HealthComponent != null)
            {
                player.HealthComponent.IsInvincible = !player.HealthComponent.IsInvincible;
                Debug.Log($"[DevConsole] God Mode toggled. Invincible = {player.HealthComponent.IsInvincible}");
            }
            else
            {
                Debug.LogWarning("[DevConsole] PlayerController or PlayerHealth not found in current scene.");
            }
        }

        private void KillAllEnemies()
        {
            var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var enemy in enemies)
            {
                if (enemy.gameObject.activeInHierarchy)
                {
                    enemy.KillEnemy();
                    count++;
                }
            }
            Debug.Log($"[DevConsole] Killed {count} active enemies.");
        }

        private void SkipRound()
        {
            var roundManager = ServiceLocator.TryGet<RoundManager>();
            if (roundManager != null)
            {
                roundManager.SkipRound();
                Debug.Log("[DevConsole] Skip Round triggered on RoundManager.");
            }
            else
            {
                Debug.LogWarning("[DevConsole] RoundManager not found in current scene.");
            }
        }

        private void HealPlayerToFull()
        {
            var player = ServiceLocator.TryGet<PlayerController>();
            if (player != null && player.HealthComponent != null)
            {
                player.HealthComponent.HealPlayer(player.HealthComponent.MaxHealth);
                Debug.Log("[DevConsole] Player health restored to full.");
            }
            else
            {
                Debug.LogWarning("[DevConsole] PlayerController or PlayerHealth not found in current scene.");
            }
        }

        private void ResetGameplayUpgrades()
        {
            var upgradesManager = ServiceLocator.TryGet<UpgradesManager>();
            if (upgradesManager != null)
            {
                upgradesManager.ResetAllUpgrades();
                Debug.Log("[DevConsole] Reset run upgrades on UpgradesManager.");
            }
            else
            {
                Debug.LogWarning("[DevConsole] UpgradesManager not found in current scene.");
            }
        }

        private void SetSkeletalLeafCurrency()
        {
            if (_currencyConfig != null && _currencyConfig.SkeletalLeafCurrency != null)
            {
                _currencyConfig.SkeletalLeafCurrency.Value = 9999;
                Debug.Log($"[DevConsole] SkeletalLeafCurrency set to {_currencyConfig.SkeletalLeafCurrency.Value}");
            }
            else
            {
                Debug.LogWarning("[DevConsole] CurrencyConfig or SkeletalLeafCurrency reference not assigned.");
            }
        }

        private void ResetPermanentUpgrades()
        {
            // 1. Reset backend SOAP variables directly in memory
            int resetCount = 0;
            foreach (var variable in _permaUpgradeVariables)
            {
                if (variable != null)
                {
                    variable.Value = 1;
                    resetCount++;
                }
            }

            // 2. Reset active/inactive UI controllers in the scene
            var uiControllers = FindObjectsByType<UpgradeItemController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var controller in uiControllers)
            {
                controller.ResetUpgrade();
            }

            Debug.Log($"[DevConsole] Reset {resetCount} permanent upgrade SOAP variables and updated {uiControllers.Length} UI items.");
        }

        // --- AUTOMATIC INSTANTIATION ---
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeDevConsole()
        {
            // Check if already instantiated
            if (_instance != null) return;

            // Load and instantiate from resources folder
            GameObject prefab = Resources.Load<GameObject>("DevConsoleCanvas");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("[DevConsole] DevConsoleCanvas successfully instantiated dynamically.");
            }
            else
            {
                Debug.LogError("[DevConsole] DevConsoleCanvas prefab could not be loaded from Resources. Please check path Assets/Resources/DevConsoleCanvas.prefab.");
            }
        }
    }
}
