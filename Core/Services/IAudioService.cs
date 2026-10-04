using System;

namespace Puzzle.Core.Services
{
    public interface IAudioService
    {
        bool IsSfxMuted { get; set; }
        bool IsMusicMuted { get; set; }
        float SfxVolume { get; set; }
        float MusicVolume { get; set; }

        void PlaySfx(string soundId);
        void PlayMusic(string trackId, bool loop = true);
        void StopMusic();
    }
}
