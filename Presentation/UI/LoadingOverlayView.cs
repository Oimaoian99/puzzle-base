using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Topmost fullscreen loading overlay used to mask level loading transitions.
    /// Blocks raycasts during asynchronous scope preparation and asset loads.
    /// </summary>
    public class LoadingOverlayView : UIWindow
    {
        [Header("Loading Visuals")]
        [SerializeField] private Text loadingText;
        [SerializeField] private Graphic spinnerGraphic;
        [SerializeField] private float spinSpeed = 180f;

        private void Update()
        {
            if (IsVisible && spinnerGraphic != null)
            {
                spinnerGraphic.rectTransform.Rotate(Vector3.forward, -spinSpeed * Time.deltaTime);
            }
        }

        public void SetLoadingMessage(string message)
        {
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }
    }
}
