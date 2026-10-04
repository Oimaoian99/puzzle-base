using System;

namespace Puzzle.Core.Level
{
    [Serializable]
    public readonly struct LevelId : IEquatable<LevelId>, IComparable<LevelId>
    {
        private readonly string _value;

        public string Value => _value ?? string.Empty;
        public bool IsValid => !string.IsNullOrEmpty(_value);
        public bool IsEmpty => string.IsNullOrEmpty(_value);

        public static readonly LevelId Empty = new LevelId(string.Empty);

        public LevelId(string value)
        {
            _value = value?.Trim() ?? string.Empty;
        }

        public LevelId(int index)
        {
            _value = index.ToString();
        }

        public bool TryGetIntIndex(out int index)
        {
            if (int.TryParse(_value, out index)) return true;

            // Also check common formats like "level_005", "level-5", "lvl_5"
            if (!string.IsNullOrEmpty(_value))
            {
                int start = -1;
                for (int i = 0; i < _value.Length; i++)
                {
                    if (char.IsDigit(_value[i]))
                    {
                        if (start < 0) start = i;
                    }
                    else if (start >= 0)
                    {
                        // Encountered non-digit after digits
                        break;
                    }
                }

                if (start >= 0)
                {
                    int len = 0;
                    while (start + len < _value.Length && char.IsDigit(_value[start + len])) len++;
                    if (int.TryParse(_value.Substring(start, len), out index))
                    {
                        return true;
                    }
                }
            }

            index = 0;
            return false;
        }

        public bool Equals(LevelId other)
        {
            return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is LevelId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_value) : 0;
        }

        public int CompareTo(LevelId other)
        {
            if (TryGetIntIndex(out var myInt) && other.TryGetIntIndex(out var otherInt))
            {
                return myInt.CompareTo(otherInt);
            }
            return string.Compare(_value, other._value, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString() => Value;

        public static bool operator ==(LevelId left, LevelId right) => left.Equals(right);
        public static bool operator !=(LevelId left, LevelId right) => !left.Equals(right);
        public static implicit operator LevelId(string val) => new LevelId(val);
        public static implicit operator LevelId(int val) => new LevelId(val);
    }
}
