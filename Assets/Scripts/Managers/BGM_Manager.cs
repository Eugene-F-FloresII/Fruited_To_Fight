using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace Managers
{
    [RequireComponent(typeof(AudioSource))]
    public class BGM_Manager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private List<AudioClip> _audioClips;
        [SerializeField] private float _fadeDuration = 1.5f;
        [Range(0f, 1f)]
        [SerializeField] private float _maxVolume = 0.5f;

        private AudioSource _audioSource;
        private List<AudioClip> _shuffledClips;
        private int _currentIndex = -1;
        private AudioClip _lastPlayedClip;
        private Tween _fadeTween;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null)
            {
                Debug.LogError("BGM_Manager: No AudioSource found on " + gameObject.name);
                return;
            }

            // Standard BGM setup
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f; // Force 2D
            _audioSource.priority = 0;      // HIGHEST PRIORITY: Never let other sounds steal BGM voice
            _audioSource.mute = false;
            _audioSource.volume = 0f;

            if (_audioClips == null || _audioClips.Count == 0)
            {
                Debug.LogWarning("BGM_Manager: No audio clips assigned in the inspector.");
                enabled = false;
                return;
            }

            _shuffledClips = new List<AudioClip>(_audioClips);
            Reshuffle();
        }

        private void Start()
        {
            if (_maxVolume <= 0)
            {
                Debug.LogWarning("BGM_Manager: Max Volume is set to 0! You won't hear anything.");
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            PlayBGMQueue(_cts.Token).Forget();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _fadeTween.Stop();
        }

        private async UniTaskVoid PlayBGMQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _currentIndex++;

                if (_currentIndex >= _shuffledClips.Count)
                {
                    Reshuffle();
                    _currentIndex = 0;
                }

                AudioClip nextClip = _shuffledClips[_currentIndex];

                if (nextClip == null)
                {
                    Debug.LogWarning("BGM_Manager: Found a null clip in the list. Skipping.");
                    continue;
                }

                // Prevent same track from playing twice in a row when reshuffling
                if (nextClip == _lastPlayedClip && _shuffledClips.Count > 1)
                {
                    _currentIndex = (_currentIndex + 1) % _shuffledClips.Count;
                    nextClip = _shuffledClips[_currentIndex];
                }

                _lastPlayedClip = nextClip;
                Debug.Log($"BGM_Manager: Starting track '{nextClip.name}' (Unscaled Time)");
                await PlayClipWithFade(nextClip, token);
            }
        }

        private async UniTask PlayClipWithFade(AudioClip clip, CancellationToken token)
        {
            // Fade out current if playing
            if (_audioSource.isPlaying)
            {
                _fadeTween.Stop();
                // Use useUnscaledTime: true for PrimeTween
                _fadeTween = Tween.AudioVolume(_audioSource, 0f, _fadeDuration, useUnscaledTime: true);
                await _fadeTween.ToUniTask(cancellationToken: token);
                _audioSource.Stop();
            }

            _audioSource.clip = clip;
            _audioSource.volume = 0f;
            _audioSource.Play();
            
            if (!_audioSource.isPlaying)
            {
                Debug.LogError($"BGM_Manager: AudioSource failed to Play() track '{clip.name}'.");
            }

            // Fade in
            _fadeTween.Stop();
            // Use useUnscaledTime: true for PrimeTween
            _fadeTween = Tween.AudioVolume(_audioSource, _maxVolume, _fadeDuration, useUnscaledTime: true);
            await _fadeTween.ToUniTask(cancellationToken: token);

            // Wait until the clip is almost finished to start the next transition
            float playDuration = clip.length - _fadeDuration;
            
            if (playDuration > 0)
            {
                // Use DelayType.UnscaledDeltaTime for UniTask
                await UniTask.Delay((int)(playDuration * 1000), delayTiming: PlayerLoopTiming.Update, cancellationToken: token, ignoreTimeScale: true);
            }
            else
            {
                await UniTask.Delay((int)(clip.length * 1000), delayTiming: PlayerLoopTiming.Update, cancellationToken: token, ignoreTimeScale: true);
            }
        }

        private void Reshuffle()
        {
            for (int i = 0; i < _shuffledClips.Count; i++)
            {
                AudioClip temp = _shuffledClips[i];
                int randomIndex = Random.Range(i, _shuffledClips.Count);
                _shuffledClips[i] = _shuffledClips[randomIndex];
                _shuffledClips[randomIndex] = temp;
            }
        }
    }
}
