using System;
using System.Collections.Generic;

namespace Puzzle.Core.Services
{
    /// <summary>
    /// Simple, resilient in-memory localization service.
    /// Supports language dictionaries, safe token replacement, and graceful key fallback when keys are missing.
    /// </summary>
    public class SimpleLocalizationService : ILocalizationService
    {
        private string _currentLanguage = "en";
        private readonly Dictionary<string, Dictionary<string, string>> _tables = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public string CurrentLanguage => _currentLanguage;

        public event Action<string> OnLanguageChanged;

        public SimpleLocalizationService() : this("en")
        {
        }

        public SimpleLocalizationService(string defaultLanguage)
        {
            _currentLanguage = defaultLanguage ?? "en";
            // Pre-seed basic default strings
            AddTranslation("en", "ui_play", "Play");
            AddTranslation("en", "ui_retry", "Retry");
            AddTranslation("en", "ui_home", "Home");
            AddTranslation("en", "ui_score", "Score: {0}");
            AddTranslation("en", "ui_moves", "Moves: {0}");
            AddTranslation("en", "ui_level_title", "Level {0}");
            AddTranslation("en", "ui_win", "Victory!");
            AddTranslation("en", "ui_lose", "Out of Moves");
        }

        public void AddTranslation(string languageCode, string key, string value)
        {
            if (string.IsNullOrEmpty(languageCode) || string.IsNullOrEmpty(key)) return;

            if (!_tables.TryGetValue(languageCode, out var dict))
            {
                dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _tables[languageCode] = dict;
            }

            dict[key] = value;
        }

        public void SetLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) return;
            if (_currentLanguage.Equals(languageCode, StringComparison.OrdinalIgnoreCase)) return;

            _currentLanguage = languageCode.ToLowerInvariant();
            OnLanguageChanged?.Invoke(_currentLanguage);
        }

        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            return _tables.TryGetValue(_currentLanguage, out var dict) && dict.ContainsKey(key);
        }

        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            // 1. Try current language
            if (_tables.TryGetValue(_currentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            {
                return val;
            }

            // 2. Fallback to default "en" if current is different
            if (!_currentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase) &&
                _tables.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out var enVal))
            {
                return enVal;
            }

            // 3. Graceful fallback: return the raw key instead of crashing
            return key;
        }

        public string GetText(string key, params object[] args)
        {
            string template = GetText(key);
            if (args == null || args.Length == 0) return template;

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                // Fallback gracefully if format specifiers mismatch
                return template;
            }
        }
    }
}
