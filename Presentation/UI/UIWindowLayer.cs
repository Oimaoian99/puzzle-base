namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Defines the rendering layer hierarchy for UI windows in the Canvas.
    /// Used by UIManager for sorting and z-order management.
    /// </summary>
    public enum UIWindowLayer
    {
        Screen = 0,   // Fullscreen screens: HomeScreen, LevelSelectScreen
        HUD = 1,      // Permanent gameplay HUD
        Popup = 2,    // Modal popups: Pause, Settings, Win, Lose (intercepts touch)
        Overlay = 3   // Topmost overlays: Loading fade, toast notifications
    }
}
