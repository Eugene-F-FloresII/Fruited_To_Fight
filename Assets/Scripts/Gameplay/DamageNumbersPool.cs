using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Shared.Events;
using UnityEngine;

namespace Gameplay
{
    public class DamageNumbersPool : MonoBehaviour
    {
        [SerializeField] private GameObject _damageNumbers;
        [SerializeField] private int _poolSize = 20;
        [Min(0.05f)]
        [SerializeField] private float _despawnDelay = 0.8f;

        [Header("Tween Settings")]
        [SerializeField] private float _popDuration = 0.12f;
        [SerializeField] private float _settleDuration = 0.1f;
        [SerializeField] private float _floatDistance = 0.35f;
        [SerializeField] private float _floatDuration = 0.4f;
        [SerializeField] private Vector2 _spawnOffsetRange = new(0.12f, 0.08f);

        [System.Serializable]
        public struct WeaponColorConfig
        {
            public Shared.Enums.WeaponClass WeaponClass;
            public Color Color;
        }

        [System.Serializable]
        public struct AfflictionColorConfig
        {
            public Shared.Enums.AfflictionType AfflictionType;
            public Color Color;
        }

        [System.Serializable]
        public struct WispColorConfig
        {
            public Shared.Enums.WispType WispType;
            public Color Color;
        }

        [Header("Damage Color Settings")]
        [SerializeField] private Color _defaultDamageColor = Color.white;
        [SerializeField] private List<WeaponColorConfig> _weaponColors = new()
        {
            new WeaponColorConfig { WeaponClass = Shared.Enums.WeaponClass.Tomahawk, Color = Color.gray }
        };
        [SerializeField] private List<AfflictionColorConfig> _afflictionColors = new()
        {
            new AfflictionColorConfig { AfflictionType = Shared.Enums.AfflictionType.Burn, Color = new Color(1f, 0.45f, 0f) },
            new AfflictionColorConfig { AfflictionType = Shared.Enums.AfflictionType.Lightning, Color = Color.yellow }
        };
        [SerializeField] private List<WispColorConfig> _wispColors = new()
        {
            new WispColorConfig { WispType = Shared.Enums.WispType.Lightning, Color = Color.yellow }
        };

        private readonly Queue<DamageNumber> _pool = new();
        private readonly HashSet<DamageNumber> _activeDamageNumbers = new();

        private void Start()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var number = CreateDamageNumber();
                if (number != null)
                {
                    _pool.Enqueue(number);
                }
            }
        }

        private void OnEnable()
        {
            Events_Enemy.OnEnemyHit += OnEnemyHit;
        }

        private void OnDisable()
        {
            Events_Enemy.OnEnemyHit -= OnEnemyHit;
        }

        private void OnEnemyHit(Vector2 position, int damage, DamageSourceInfo sourceInfo)
        {
            SpawnDamageNumberAsync(position, damage, sourceInfo).Forget();
        }

        private async UniTaskVoid SpawnDamageNumberAsync(Vector2 position, int damage, DamageSourceInfo sourceInfo)
        {
            var damageNumber = GetDamageNumber();
            if (damageNumber == null)
            {
                return;
            }

            _activeDamageNumbers.Add(damageNumber);
            var damageNumberTransform = damageNumber.transform;
            damageNumberTransform.position = position + GetRandomOffset();

            damageNumber.gameObject.SetActive(true);
            
            Color textColor = GetDamageColor(sourceInfo);
            damageNumber.InitiateDamageNumber(damage, textColor);
            
            damageNumber.PlayShowTweenAsync(_popDuration, _settleDuration, _floatDistance, _floatDuration).Forget();

            try
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(_despawnDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
                Return(damageNumber);
            }
            catch (System.OperationCanceledException)
            {
                Return(damageNumber);
            }
        }

        public Color GetDamageColor(DamageSourceInfo sourceInfo)
        {
            if (sourceInfo.IsAffliction)
            {
                foreach (var config in _afflictionColors)
                {
                    if (config.AfflictionType == sourceInfo.AfflictionType)
                        return config.Color;
                }
            }

            if (sourceInfo.IsWeapon)
            {
                if (sourceInfo.AfflictionType != Shared.Enums.AfflictionType.None)
                {
                    foreach (var config in _afflictionColors)
                    {
                        if (config.AfflictionType == sourceInfo.AfflictionType)
                            return config.Color;
                    }
                }

                foreach (var config in _weaponColors)
                {
                    if (config.WeaponClass == sourceInfo.WeaponClass)
                        return config.Color;
                }
            }

            if (sourceInfo.IsWisp)
            {
                foreach (var config in _wispColors)
                {
                    if (config.WispType == sourceInfo.WispType)
                        return config.Color;
                }
            }

            return _defaultDamageColor;
        }

        public void Return(DamageNumber damageNumber)
        {
            if (damageNumber == null)
            {
                return;
            }

            if (!_activeDamageNumbers.Remove(damageNumber))
            {
                return;
            }

            damageNumber.gameObject.SetActive(false);
            _pool.Enqueue(damageNumber);
        }

        private DamageNumber GetDamageNumber()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }

            return CreateDamageNumber();
        }

        private DamageNumber CreateDamageNumber()
        {
            if (_damageNumbers == null)
            {
                Debug.LogError("Damage number prefab is not assigned.", this);
                return null;
            }

            var obj = Instantiate(_damageNumbers, transform);
            obj.SetActive(false);

            if (obj.TryGetComponent(out DamageNumber damageNumber))
            {
                return damageNumber;
            }

            Debug.LogError("Damage number prefab is missing the DamageNumber component.", obj);
            Destroy(obj);
            return null;
        }

        private Vector2 GetRandomOffset()
        {
            return new Vector3(
                Random.Range(-_spawnOffsetRange.x, _spawnOffsetRange.x),
                Random.Range(-_spawnOffsetRange.y, _spawnOffsetRange.y),
                0f);
        }
    }
}
