using System;

namespace Puzzle.Core.Logging
{
    /// <summary>
    /// Lightweight logging bridge for Core.
    /// Dispatches to UnityEngine.Debug when inside the Unity engine,
    /// or safely falls back to System.Console in standalone .NET unit test runners.
    /// </summary>
    public static class CoreLogger
    {
        private static bool _useConsoleFallback;

        public static Action<string> CustomLogHandler { get; set; }
        public static Action<string> CustomLogWarningHandler { get; set; }
        public static Action<string> CustomLogErrorHandler { get; set; }

        public static void Log(string message)
        {
            if (CustomLogHandler != null)
            {
                CustomLogHandler(message);
                return;
            }

            if (_useConsoleFallback)
            {
                Console.WriteLine(message);
                return;
            }

            try
            {
                UnityEngine.Debug.Log(message);
            }
            catch (Exception)
            {
                _useConsoleFallback = true;
                Console.WriteLine(message);
            }
        }

        public static void LogWarning(string message)
        {
            if (CustomLogWarningHandler != null)
            {
                CustomLogWarningHandler(message);
                return;
            }

            if (_useConsoleFallback)
            {
                Console.WriteLine("[WARN] " + message);
                return;
            }

            try
            {
                UnityEngine.Debug.LogWarning(message);
            }
            catch (Exception)
            {
                _useConsoleFallback = true;
                Console.WriteLine("[WARN] " + message);
            }
        }

        public static void LogError(string message)
        {
            if (CustomLogErrorHandler != null)
            {
                CustomLogErrorHandler(message);
                return;
            }

            if (_useConsoleFallback)
            {
                Console.Error.WriteLine("[ERROR] " + message);
                return;
            }

            try
            {
                UnityEngine.Debug.LogError(message);
            }
            catch (Exception)
            {
                _useConsoleFallback = true;
                Console.Error.WriteLine("[ERROR] " + message);
            }
        }
    }
}
