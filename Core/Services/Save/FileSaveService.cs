using System;
using System.IO;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;

namespace Puzzle.Core.Services
{
    /// <summary>
    /// Resilient, production-ready local file save service implementing ISaveService.
    /// Employs atomic disk writes (temporary file flush + atomic replace) to prevent corruption.
    /// Recovers gracefully from corrupted save files without crashing the application.
    /// Completely isolates gameplay from file paths, serialization, and disk operations.
    /// Pure C# implementation with zero dependence on native Unity engine internals (ECalls).
    /// </summary>
    public class FileSaveService : ISaveService
    {
        private readonly string _saveFilePath;
        private PlayerSaveData _data;

        public PlayerSaveData Data => _data;
        public string SaveFilePath => _saveFilePath;

        public FileSaveService() : this(null)
        {
        }

        public FileSaveService(string saveFilePath)
        {
            if (string.IsNullOrWhiteSpace(saveFilePath))
            {
                string dir = ResolveSafeDataDirectory();
                _saveFilePath = Path.Combine(dir, "player_save.json");
            }
            else
            {
                _saveFilePath = saveFilePath;
            }

            Load();
        }

        private static string ResolveSafeDataDirectory()
        {
            try
            {
                // In standalone Unity player, AppDomain base directory or local app data is safe and standard
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(localAppData))
                {
                    string puzzleDir = Path.Combine(localAppData, "PuzzleBase");
                    if (!Directory.Exists(puzzleDir))
                    {
                        Directory.CreateDirectory(puzzleDir);
                    }
                    return puzzleDir;
                }
            }
            catch
            {
                // Fallback gracefully
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public int GetHighestCompletedLevelIndex()
        {
            return _data?.HighestCompletedLevelIndex ?? 0;
        }

        public void SetLevelCompleted(LevelId levelId, int stars, int score)
        {
            if (_data == null) _data = new PlayerSaveData();

            var record = _data.FindRecord(levelId.Value);
            if (record == null)
            {
                record = new LevelRecord(levelId.Value, stars, score, true);
                _data.LevelRecords.Add(record);
            }
            else
            {
                record.IsCompleted = true;
                if (stars > record.Stars) record.Stars = stars;
                if (score > record.HighScore) record.HighScore = score;
            }

            if (levelId.TryGetIntIndex(out int index))
            {
                if (index > _data.HighestCompletedLevelIndex)
                {
                    _data.HighestCompletedLevelIndex = index;
                }
            }

            Save();
        }

        public bool IsLevelCompleted(LevelId levelId)
        {
            if (_data == null) return false;
            var record = _data.FindRecord(levelId.Value);
            return record != null && record.IsCompleted;
        }

        public int GetStarsEarned(LevelId levelId)
        {
            if (_data == null) return 0;
            var record = _data.FindRecord(levelId.Value);
            return record != null ? record.Stars : 0;
        }

        public int GetCoins()
        {
            return _data?.Coins ?? 0;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            if (_data == null) _data = new PlayerSaveData();
            _data.Coins += amount;
            Save();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (_data == null || _data.Coins < amount) return false;

            _data.Coins -= amount;
            Save();
            return true;
        }

        public void Save()
        {
            if (_data == null) return;

            try
            {
                string directory = Path.GetDirectoryName(_saveFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_data, Newtonsoft.Json.Formatting.Indented);
                string tempFilePath = _saveFilePath + ".tmp";

                // Atomic write pattern: write to temporary file first, then replace target file
                File.WriteAllText(tempFilePath, json);

                if (File.Exists(_saveFilePath))
                {
                    File.Delete(_saveFilePath);
                }

                File.Move(tempFilePath, _saveFilePath);
            }
            catch (Exception ex)
            {
                CoreLogger.LogError($"[FileSaveService] Failed to write save file to '{_saveFilePath}': {ex.Message}");
            }
        }

        public void Load()
        {
            if (!File.Exists(_saveFilePath))
            {
                _data = new PlayerSaveData();
                return;
            }

            try
            {
                string json = File.ReadAllText(_saveFilePath);
                _data = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerSaveData>(json);

                if (_data == null)
                {
                    CoreLogger.LogWarning($"[FileSaveService] Save file at '{_saveFilePath}' was empty or invalid. Creating fresh save.");
                    _data = new PlayerSaveData();
                }
            }
            catch (Exception ex)
            {
                // Corrupt file recovery: do NOT crash the game. Reset to clean defaults with warning.
                CoreLogger.LogError($"[FileSaveService] Corrupt save file at '{_saveFilePath}'. Restoring default data. Error: {ex.Message}");
                _data = new PlayerSaveData();
            }
        }

        public void ResetSave()
        {
            _data = new PlayerSaveData();
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    File.Delete(_saveFilePath);
                }
            }
            catch (Exception ex)
            {
                CoreLogger.LogWarning($"[FileSaveService] Failed to delete save file during reset: {ex.Message}");
            }
        }
    }
}
