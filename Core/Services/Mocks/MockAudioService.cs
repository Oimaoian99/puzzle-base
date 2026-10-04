using System.Collections.Generic;
using Puzzle.Core.Logging;

namespace Puzzle.Core.Services.Mocks
{
    public class MockAudioService : IAudioService
    {
        public bool IsSfxMuted { get; set; } = false;
        public bool IsMusicMuted { get; set; } = false;
        public float SfxVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 1.0f;

        public string CurrentlyPlayingMusic { get; private set; }
        public List<string> PlayedSfxLog { get; } = new List<string>();

        public void PlaySfx(string soundId)
        {
            if (IsSfxMuted) return;
            PlayedSfxLog.Add(soundId);
            CoreLogger.Log($"[MockAudioService] Played SFX: {soundId}");
        }

        public void PlayMusic(string trackId, bool loop = true)
        {
            if (IsMusicMuted) return;
            CurrentlyPlayingMusic = trackId;
            CoreLogger.Log($"[MockAudioService] Playing Music: {trackId} (loop: {loop})");
        }

        public void StopMusic()
        {
            CurrentlyPlayingMusic = null;
            CoreLogger.Log("[MockAudioService] Stopped Music");
        }
    }
}
