using System;
using Puzzle.Core.GameFlow;
using Puzzle.Core.Level;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Service boundary interface for UI management.
    /// Manages window retrieval, visibility transitions, and modal dialog stacks.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// Registers a UIWindow with the manager.
        /// </summary>
        void RegisterWindow(UIWindow window);

        /// <summary>
        /// Unregisters a UIWindow from the manager.
        /// </summary>
        void UnregisterWindow(UIWindow window);

        /// <summary>
        /// Gets a registered window by type.
        /// </summary>
        T GetWindow<T>() where T : UIWindow;

        /// <summary>
        /// Shows a window of the specified type.
        /// </summary>
        T ShowWindow<T>() where T : UIWindow;

        /// <summary>
        /// Hides a window of the specified type.
        /// </summary>
        void HideWindow<T>() where T : UIWindow;

        /// <summary>
        /// Hides all windows registered on a specific layer (e.g., all Popups).
        /// </summary>
        void HideLayer(UIWindowLayer layer);

        /// <summary>
        /// Hides all currently open popups.
        /// </summary>
        void HideAllPopups();

        /// <summary>
        /// Activates the in-game HUD and binds it to the level runtime and state machine.
        /// </summary>
        void ShowHUD(ILevelRuntime runtime, IGameStateMachine fsm, int targetScore = 0);

        /// <summary>
        /// Activates the victory popup and binds it to the result data.
        /// </summary>
        void ShowWin(LevelResultData result, IGameStateMachine fsm);

        /// <summary>
        /// Activates the defeat popup and binds it to the result data.
        /// </summary>
        void ShowLose(LevelResultData result, IGameStateMachine fsm);

        /// <summary>
        /// Activates the pause popup.
        /// </summary>
        void ShowPause(IGameStateMachine fsm);

        /// <summary>
        /// Activates the main menu home screen.
        /// </summary>
        void ShowHome(IGameStateMachine fsm);
    }
}
