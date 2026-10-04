using System;
using UnityEngine;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Base class for all UI elements managed by UIManager.
    /// Provides standard show/hide semantics, visibility tracking, and optional CanvasGroup fading.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIWindow : MonoBehaviour
    {
        [Header("Window Configuration")]
        [SerializeField] private UIWindowLayer layer = UIWindowLayer.Screen;
        [SerializeField] private CanvasGroup canvasGroup;

        public UIWindowLayer Layer => layer;
        public bool IsVisible { get; private set; }

        public event Action<UIWindow> OnVisibleChanged;

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Makes this window visible and interactive.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            IsVisible = true;
            OnShown();
            OnVisibleChanged?.Invoke(this);
        }

        /// <summary>
        /// Hides this window and disables raycasts.
        /// </summary>
        public virtual void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
            IsVisible = false;
            OnHidden();
            OnVisibleChanged?.Invoke(this);
        }

        /// <summary>
        /// Toggles window visibility between Show and Hide.
        /// </summary>
        public void Toggle()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
        protected virtual void OnDestroy() { }
    }
}
