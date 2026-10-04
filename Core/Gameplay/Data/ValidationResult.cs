using System;
using System.Collections.Generic;
using System.Text;

namespace Puzzle.Core.Gameplay.Data
{
    /// <summary>
    /// Represents the outcome of a level data validation pass.
    /// Carries descriptive error and warning messages without throwing exceptions.
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors;
        public IReadOnlyList<string> Warnings => _warnings;

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _errors.Add(message.Trim());
            }
        }

        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _warnings.Add(message.Trim());
            }
        }

        public override string ToString()
        {
            if (IsValid && _warnings.Count == 0)
            {
                return "Validation Passed (0 errors, 0 warnings).";
            }

            var sb = new StringBuilder();
            if (_errors.Count > 0)
            {
                sb.AppendLine($"[Validation Failed] {_errors.Count} error(s):");
                for (int i = 0; i < _errors.Count; i++)
                {
                    sb.AppendLine($"  ERROR #{i + 1}: {_errors[i]}");
                }
            }

            if (_warnings.Count > 0)
            {
                sb.AppendLine($"[Validation Warnings] {_warnings.Count} warning(s):");
                for (int i = 0; i < _warnings.Count; i++)
                {
                    sb.AppendLine($"  WARN #{i + 1}: {_warnings[i]}");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
