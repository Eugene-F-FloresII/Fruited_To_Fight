using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Shared.Events;
using UnityEngine;

namespace Managers
{
    public class SFXManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private float _minPitch = 0.9f;
        [SerializeField] private float _maxPitch = 1.1f;
        [SerializeField] private float _minVolume = 0.8f;
        [SerializeField] private float _maxVolume = 1.0f;

        private Queue<AudioSource> _pool = new Queue<AudioSource>();
        private Transform _poolParent;

        private void Awake()
        {
            _poolParent = new GameObject("SFX_Pool").transform;
            _poolParent.SetParent(this.transform);

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewAudioSource();
            }
        }

        private void OnEnable()
        {
            Events_Sound.PlaySound += OnPlaySound;
        }

        private void OnDisable()
        {
            Events_Sound.PlaySound -= OnPlaySound;
        }

        private void OnPlaySound(AudioClip clip)
        {
            if (clip == null) return;
            PlaySoundAsync(clip, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid PlaySoundAsync(AudioClip clip, System.Threading.CancellationToken cancellationToken)
        {
            AudioSource source = GetSource();
            
            // Randomize properties
            source.clip = clip;
            source.pitch = Random.Range(_minPitch, _maxPitch);
            source.volume = Random.Range(_minVolume, _maxVolume);
            
            source.gameObject.SetActive(true);
            source.Play();

            // Wait until the clip finishes (using unscaled time to be safe)
            bool canceled = await UniTask.Delay((int)(clip.length * 1000), ignoreTimeScale: true, cancellationToken: cancellationToken).SuppressCancellationThrow();

            if (canceled || source == null) return;

            // Return to pool
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _pool.Enqueue(source);
        }

        private AudioSource GetSource()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }

            return CreateNewAudioSource(false);
        }

        private AudioSource CreateNewAudioSource(bool startInPool = true)
        {
            GameObject go = new GameObject("SFX_Player");
            go.transform.SetParent(_poolParent);
            
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D sound
            
            go.SetActive(false);

            if (startInPool)
            {
                _pool.Enqueue(source);
            }
            
            return source;
        }
    }
}
