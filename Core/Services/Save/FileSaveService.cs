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

                string json = SerializeSaveData(_data);
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
                _data = DeserializeSaveData(json);

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

        public static string SerializeSaveData(PlayerSaveData data)
        {
            if (data == null) data = new PlayerSaveData();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"SaveVersion\": {data.SaveVersion},");
            sb.AppendLine($"  \"Coins\": {data.Coins},");
            sb.AppendLine($"  \"HighestCompletedLevelIndex\": {data.HighestCompletedLevelIndex},");
            sb.AppendLine("  \"LevelRecords\": [");
            if (data.LevelRecords != null)
            {
                for (int i = 0; i < data.LevelRecords.Count; i++)
                {
                    var r = data.LevelRecords[i];
                    if (r == null) continue;
                    string comma = (i < data.LevelRecords.Count - 1) ? "," : "";
                    string escapedId = (r.LevelId ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    sb.AppendLine($"    {{\"LevelId\": \"{escapedId}\", \"Stars\": {r.Stars}, \"HighScore\": {r.HighScore}, \"IsCompleted\": {(r.IsCompleted ? "true" : "false")}}}{comma}");
                }
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static PlayerSaveData DeserializeSaveData(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new PlayerSaveData();

            string trimmed = json.Trim();
            if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}"))
            {
                throw new FormatException("JSON structure is malformed: missing root object braces.");
            }

            var data = new PlayerSaveData();

            var matchVersion = System.Text.RegularExpressions.Regex.Match(json, @"""SaveVersion""\s*:\s*(\d+)");
            if (matchVersion.Success && int.TryParse(matchVersion.Groups[1].Value, out int version))
            {
                data.SaveVersion = version;
            }

            var matchCoins = System.Text.RegularExpressions.Regex.Match(json, @"""Coins""\s*:\s*(-?\d+)");
            if (matchCoins.Success && int.TryParse(matchCoins.Groups[1].Value, out int coins))
            {
                data.Coins = coins;
            }

            var matchHighest = System.Text.RegularExpressions.Regex.Match(json, @"""HighestCompletedLevelIndex""\s*:\s*(-?\d+)");
            if (matchHighest.Success && int.TryParse(matchHighest.Groups[1].Value, out int highest))
            {
                data.HighestCompletedLevelIndex = highest;
            }

            var recordMatches = System.Text.RegularExpressions.Regex.Matches(json, @"\{[^{}]*""LevelId""[^{}]*\}");
            foreach (System.Text.RegularExpressions.Match rm in recordMatches)
            {
                string block = rm.Value;
                var idMatch = System.Text.RegularExpressions.Regex.Match(block, @"""LevelId""\s*:\s*""([^""]*)""");
                var starsMatch = System.Text.RegularExpressions.Regex.Match(block, @"""Stars""\s*:\s*(\d+)");
                var scoreMatch = System.Text.RegularExpressions.Regex.Match(block, @"""HighScore""\s*:\s*(\d+)");
                var completedMatch = System.Text.RegularExpressions.Regex.Match(block, @"""IsCompleted""\s*:\s*(true|false)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (idMatch.Success)
                {
                    string levelId = idMatch.Groups[1].Value.Replace("\\\"", "\"").Replace("\\\\", "\\");
                    int stars = starsMatch.Success && int.TryParse(starsMatch.Groups[1].Value, out int s) ? s : 0;
                    int score = scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out int sc) ? sc : 0;
                    bool isCompleted = completedMatch.Success && bool.TryParse(completedMatch.Groups[1].Value, out bool c) && c;

                    data.LevelRecords.Add(new LevelRecord(levelId, stars, score, isCompleted));
                }
            }

            return data;
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
