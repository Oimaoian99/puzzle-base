using System;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Services;
using Puzzle.Presentation.VFX;
using UnityEngine;

namespace Puzzle.Presentation.Feedback
{
    /// <summary>
    /// Central feedback coordinator in the Presentation layer.
    /// Bridges high-level FeedbackEvent triggers to Audio, VFX, and Haptic services.
    /// Resilient: if Audio, VFX, or Haptics are null or throw errors, gameplay continues unaffected.
    /// Pure C# presentation class - independent of MonoBehaviour lifecycle, easily testable and DI injectable.
    /// </summary>
    public class FeedbackPresenter : IFeedbackPresenter
    {
        private IAudioService _audioService;
        private IHapticService _hapticService;
        private IVfxService _vfxService;
        private Func<GridPosition, Vector3> _gridToWorldFunc;

        public FeedbackPresenter(
            IAudioService audioService = null,
            IHapticService hapticService = null,
            IVfxService vfxService = null,
            Func<GridPosition, Vector3> gridToWorldFunc = null)
        {
            Initialize(audioService, hapticService, vfxService, gridToWorldFunc);
        }

        public void Initialize(
            IAudioService audioService,
            IHapticService hapticService,
            IVfxService vfxService,
            Func<GridPosition, Vector3> gridToWorldFunc = null)
        {
            _audioService = audioService;
            _hapticService = hapticService;
            _vfxService = vfxService;
            _gridToWorldFunc = gridToWorldFunc;
        }

        public void HandleFeedback(in FeedbackEvent evt)
        {
            Vector3 worldPos = _gridToWorldFunc != null ? _gridToWorldFunc(evt.Position) : Vector3.zero;

            switch (evt.Type)
            {
                case FeedbackTriggerType.PieceSelected:
                    _audioService?.PlaySfx("piece_select");
                    _hapticService?.TriggerHaptic(HapticFeedbackType.Light);
                    _vfxService?.SpawnPieceVfx("select_sparkle", worldPos, evt.PieceType);
                    break;

                case FeedbackTriggerType.PieceDeselected:
                    _audioService?.PlaySfx("piece_deselect");
                    break;

                case FeedbackTriggerType.PieceMatched:
                    _audioService?.PlaySfx("piece_match");
                    _hapticService?.TriggerHaptic(HapticFeedbackType.Medium);
                    _vfxService?.SpawnPieceVfx("match_burst", worldPos, evt.PieceType);
                    break;

                case FeedbackTriggerType.PieceRemoved:
                    _audioService?.PlaySfx("piece_pop");
                    _vfxService?.SpawnPieceVfx("pop_dust", worldPos, evt.PieceType);
                    break;

                case FeedbackTriggerType.PieceMoved:
                    _audioService?.PlaySfx("piece_slide");
                    break;

                case FeedbackTriggerType.LevelStarted:
                    _audioService?.PlayMusic("bgm_gameplay");
                    break;

                case FeedbackTriggerType.LevelCompleted:
                    _audioService?.PlaySfx("level_win");
                    _hapticService?.TriggerHaptic(HapticFeedbackType.Success);
                    _vfxService?.SpawnScreenVfx("win_confetti");
                    break;

                case FeedbackTriggerType.LevelFailed:
                    _audioService?.PlaySfx("level_fail");
                    _hapticService?.TriggerHaptic(HapticFeedbackType.Failure);
                    break;
            }
        }
    }
}
