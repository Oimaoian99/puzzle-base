using System.Collections.Generic;
using Puzzle.Core.Logging;
using Puzzle.Core.Services;
using UnityEngine;

namespace Puzzle.Presentation.Audio
{
    /// <summary>
    /// Presentation-layer audio adapter implementing IAudioService.
    /// Bridges abstract gameplay audio requests to Unity presentation (AudioSource/AudioClip).
    /// Pure C# gameplay code never touches this class or any UnityEngine.Audio APIs.
    /// </summary>
    public class UnityAudioService : MonoBehaviour, IAudioService
    {
        [System.Serializable]
        public struct SoundEntry
        {
            public string SoundId;
            public AudioClip Clip;
        }

        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private List<SoundEntry> _soundBank = new List<SoundEntry>();

        private readonly Dictionary<string, AudioClip> _clipLookup = new Dictionary<string, AudioClip>();

        private bool _isSfxMuted;
        private bool _isMusicMuted;
        private float _sfxVolume = 1.0f;
        private float _musicVolume = 1.0f;

        public bool IsSfxMuted
        {
            get => _isSfxMuted;
            set
            {
                _isSfxMuted = value;
                if (_sfxSource != null) _sfxSource.mute = value;
            }
        }

        public bool IsMusicMuted
        {
            get => _isMusicMuted;
            set
            {
                _isMusicMuted = value;
                if (_musicSource != null) _musicSource.mute = value;
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                if (_sfxSource != null) _sfxSource.volume = _sfxVolume;
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (_musicSource != null) _musicSource.volume = _musicVolume;
            }
        }

        private void Awake()
        {
            RebuildLookup();
        }

        public void RebuildLookup()
        {
            _clipLookup.Clear();
            if (_soundBank != null)
            {
                foreach (var entry in _soundBank)
                {
                    if (!string.IsNullOrEmpty(entry.SoundId) && entry.Clip != null)
                    {
                        _clipLookup[entry.SoundId] = entry.Clip;
                    }
                }
            }
        }

        public void PlaySfx(string soundId)
        {
            if (IsSfxMuted || string.IsNullOrEmpty(soundId)) return;

            if (_clipLookup.TryGetValue(soundId, out var clip) && clip != null)
            {
                if (_sfxSource != null)
                {
                    _sfxSource.PlayOneShot(clip, _sfxVolume);
                }
            }
            else
            {
                CoreLogger.LogWarning($"[UnityAudioService] SFX '{soundId}' not found in sound bank.");
            }
        }

        public void PlayMusic(string trackId, bool loop = true)
        {
            if (IsMusicMuted || string.IsNullOrEmpty(trackId)) return;

            if (_clipLookup.TryGetValue(trackId, out var clip) && clip != null)
            {
                if (_musicSource != null)
                {
                    _musicSource.clip = clip;
                    _musicSource.loop = loop;
                    _musicSource.volume = _musicVolume;
                    _musicSource.Play();
                }
            }
            else
            {
                CoreLogger.LogWarning($"[UnityAudioService] Music track '{trackId}' not found in sound bank.");
            }
        }

        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }
    }
}
