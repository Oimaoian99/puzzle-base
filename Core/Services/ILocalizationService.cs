using System;

namespace Puzzle.Core.Services
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        
        string GetText(string key);
        string GetText(string key, params object[] args);
        bool HasKey(string key);
        void SetLanguage(string languageCode);
        
        event Action<string> OnLanguageChanged;
    }
}
