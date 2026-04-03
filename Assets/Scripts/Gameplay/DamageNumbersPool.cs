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

        private void OnEnemyHit(Vector2 arg1, int arg2)
        {
            SpawnDamageNumberAsync(arg1, arg2).Forget();
        }

        private async UniTaskVoid SpawnDamageNumberAsync(Vector2 position, int damage)
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
            damageNumber.InitiateDamageNumber(damage);
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
