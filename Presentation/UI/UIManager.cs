using System;
using System.Collections.Generic;
using Puzzle.Core.GameFlow;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;
using UnityEngine;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Central UI orchestrator managing all registered screens, HUDs, and popups.
    /// Supports automatic child discovery on Awake, layer-based hiding, and type-safe window lookup.
    /// </summary>
    public class UIManager : MonoBehaviour, IUIManager
    {
        [Header("Auto Discovery")]
        [SerializeField] private bool autoDiscoverChildren = true;

        [Header("Initial Windows")]
        [SerializeField] private List<UIWindow> initialWindows = new List<UIWindow>();

        private readonly Dictionary<Type, UIWindow> _windowsByType = new Dictionary<Type, UIWindow>();
        private readonly List<UIWindow> _allWindows = new List<UIWindow>();

        private void Awake()
        {
            if (autoDiscoverChildren)
            {
                var discovered = GetComponentsInChildren<UIWindow>(true);
                foreach (var window in discovered)
                {
                    RegisterWindow(window);
                }
            }
            else
            {
                foreach (var window in initialWindows)
                {
                    if (window != null)
                    {
                        RegisterWindow(window);
                    }
                }
            }
        }

        public void RegisterWindow(UIWindow window)
        {
            if (window == null) return;

            var type = window.GetType();
            if (!_windowsByType.ContainsKey(type))
            {
                _windowsByType[type] = window;
                _allWindows.Add(window);
            }
        }

        public void UnregisterWindow(UIWindow window)
        {
            if (window == null) return;

            var type = window.GetType();
            if (_windowsByType.ContainsKey(type))
            {
                _windowsByType.Remove(type);
                _allWindows.Remove(window);
            }
        }

        public T GetWindow<T>() where T : UIWindow
        {
            if (_windowsByType.TryGetValue(typeof(T), out var window))
            {
                return (T)window;
            }

            CoreLogger.LogWarning($"[UIManager] Window of type '{typeof(T).Name}' not registered.");
            return null;
        }

        public T ShowWindow<T>() where T : UIWindow
        {
            var window = GetWindow<T>();
            if (window != null)
            {
                window.Show();
            }
            return window;
        }

        public void HideWindow<T>() where T : UIWindow
        {
            var window = GetWindow<T>();
            if (window != null)
            {
                window.Hide();
            }
        }

        public void HideLayer(UIWindowLayer layer)
        {
            for (int i = 0; i < _allWindows.Count; i++)
            {
                if (_allWindows[i].Layer == layer && _allWindows[i].IsVisible)
                {
                    _allWindows[i].Hide();
                }
            }
        }

        public void HideAllPopups()
        {
            HideLayer(UIWindowLayer.Popup);
        }

        public void ShowHUD(ILevelRuntime runtime, IGameStateMachine fsm, int targetScore = 0)
        {
            HideLayer(UIWindowLayer.Screen);
            HideLayer(UIWindowLayer.Popup);

            var hud = GetWindow<GameplayHUDView>();
            if (hud != null)
            {
                hud.Bind(runtime, fsm, targetScore);
                hud.Show();
            }
        }

        public void ShowWin(LevelResultData result, IGameStateMachine fsm)
        {
            HideLayer(UIWindowLayer.Popup);

            var winPopup = GetWindow<WinResultPopupView>();
            if (winPopup != null)
            {
                winPopup.Bind(result, fsm);
            }
        }

        public void ShowLose(LevelResultData result, IGameStateMachine fsm)
        {
            HideLayer(UIWindowLayer.Popup);

            var losePopup = GetWindow<LoseResultPopupView>();
            if (losePopup != null)
            {
                losePopup.Bind(result, fsm);
            }
        }

        public void ShowPause(IGameStateMachine fsm)
        {
            var pausePopup = GetWindow<PausePopupView>();
            if (pausePopup != null)
            {
                pausePopup.Bind(fsm);
                pausePopup.Show();
            }
        }

        public void ShowHome(IGameStateMachine fsm)
        {
            HideLayer(UIWindowLayer.HUD);
            HideLayer(UIWindowLayer.Popup);

            var home = GetWindow<HomeScreenView>();
            if (home != null)
            {
                home.Bind(fsm);
                home.Show();
            }
        }
    }
}
