using System;
using System.Collections.Generic;
using Puzzle.Core.GameFlow;
using Puzzle.Core.Level;
using Puzzle.Presentation.UI;

namespace Puzzle.Tests.Presentation.Mocks
{
    /// <summary>
    /// Headless mock implementation of IUIManager for pure C# automated testing without requiring Unity GameObject bindings.
    /// </summary>
    public class MockUIManager : IUIManager
    {
        private readonly Dictionary<Type, UIWindow> _windows = new Dictionary<Type, UIWindow>();
        private readonly HashSet<Type> _visibleWindowTypes = new HashSet<Type>();

        public int RegisteredCount => _windows.Count;
        public int VisibleCount => _visibleWindowTypes.Count;

        public bool WasHUDShown { get; private set; }
        public bool WasWinShown { get; private set; }
        public bool WasLoseShown { get; private set; }
        public bool WasPauseShown { get; private set; }
        public bool WasHomeShown { get; private set; }

        public LevelResultData LastResult { get; private set; }
        public ILevelRuntime LastBoundRuntime { get; private set; }

        public void RegisterWindow(UIWindow window)
        {
            if (window == null) return;
            _windows[window.GetType()] = window;
        }

        public void UnregisterWindow(UIWindow window)
        {
            if (window == null) return;
            var type = window.GetType();
            _windows.Remove(type);
            _visibleWindowTypes.Remove(type);
        }

        public T GetWindow<T>() where T : UIWindow
        {
            if (_windows.TryGetValue(typeof(T), out var w))
                return (T)w;
            return null;
        }

        public T ShowWindow<T>() where T : UIWindow
        {
            _visibleWindowTypes.Add(typeof(T));
            return GetWindow<T>();
        }

        public void HideWindow<T>() where T : UIWindow
        {
            _visibleWindowTypes.Remove(typeof(T));
        }

        public void HideLayer(UIWindowLayer layer)
        {
            // In headless mock, clear visible entries
            _visibleWindowTypes.Clear();
        }

        public void HideAllPopups()
        {
            _visibleWindowTypes.Remove(typeof(PausePopupView));
            _visibleWindowTypes.Remove(typeof(WinResultPopupView));
            _visibleWindowTypes.Remove(typeof(LoseResultPopupView));
        }

        public void ShowHUD(ILevelRuntime runtime, IGameStateMachine fsm, int targetScore = 0)
        {
            WasHUDShown = true;
            LastBoundRuntime = runtime;
            _visibleWindowTypes.Add(typeof(GameplayHUDView));
        }

        public void ShowWin(LevelResultData result, IGameStateMachine fsm)
        {
            WasWinShown = true;
            LastResult = result;
            _visibleWindowTypes.Add(typeof(WinResultPopupView));
        }

        public void ShowLose(LevelResultData result, IGameStateMachine fsm)
        {
            WasLoseShown = true;
            LastResult = result;
            _visibleWindowTypes.Add(typeof(LoseResultPopupView));
        }

        public void ShowPause(IGameStateMachine fsm)
        {
            WasPauseShown = true;
            _visibleWindowTypes.Add(typeof(PausePopupView));
        }

        public void ShowHome(IGameStateMachine fsm)
        {
            WasHomeShown = true;
            _visibleWindowTypes.Add(typeof(HomeScreenView));
        }

        public bool IsWindowVisible<T>() where T : UIWindow
        {
            return _visibleWindowTypes.Contains(typeof(T));
        }
    }
}
