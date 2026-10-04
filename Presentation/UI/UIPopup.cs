using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Specialized UIWindow representing a modal popup/dialog that dims background interaction.
    /// </summary>
    public abstract class UIPopup : UIWindow
    {
        [Header("Popup Specifics")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Graphic dimmerGraphic;

        public event Action OnClosed;

        protected override void Awake()
        {
            base.Awake();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        public virtual void Close()
        {
            Hide();
            OnClosed?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }
    }
}
