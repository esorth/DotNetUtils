// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2024 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see http://creativecommons.org/publicdomain/zero/1.0/.

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DotNetUtils
{
    public static class RangeExtensions
    {
        // Adds `val` to the offset represented by the Index.
        public static Index Add(this Index idx, int val)
        {
            if (idx.IsFromEnd)
            {
                return ^(idx.Value - val);
            }
            else
            {
                return idx.Value + val;
            }
        }

        // Whether or not the Index is less than `other`. Returns null if the answer cannot be determined.
        public static bool? LessThan(this Index idx, Index other)
        {
            if (idx.IsFromEnd && other.IsFromEnd)
            {
                return idx.Value > other.Value;
            }
            else if (!idx.IsFromEnd && !other.IsFromEnd)
            {
                return idx.Value < other.Value;
            }
            else
            {
                return null;
            }
        }

        // Given an Index `i` nested within the Range (e.g. indexing so that offset 0 is `Start`), returns
        // whether or not `i` is within the Range. Returns null if the answer cannot be determined. Validates
        // range is valid when able (but not always able).
        public static bool? NestedContains(this Range r, Index i)
        {
            bool? rangeInvalid = r.End.LessThan(r.Start);
            if (rangeInvalid.GetValueOrDefault(false))
            {
                throw new InvalidOperationException("Invalid range.");
            }

            // Either before `r.Start` or after `r.End`.
            if (i.Value < 0) return false;

            if (r.Start.IsFromEnd && r.End.IsFromEnd && i.IsFromEnd)
            {
                return i.Value <= r.Start.Value - r.End.Value;
            }
            else if (!r.Start.IsFromEnd && !r.End.IsFromEnd && !i.IsFromEnd)
            {
                return i.Value <= r.End.Value - r.Start.Value;
            }
            else
            {
                return null;
            }
        }

        // Takes an Index `index` nested within the range (e.g. indexing so that offset 0 is `Start`), and
        // converts to an unnested Index (with the same offset space as the range). Validates that the range
        // is valid and that `index` is contained within the range when able (but not always able).
        public static Index UnNest(this Range outer, Index index)
        {
            bool? rangeInvalid = outer.End.LessThan(outer.Start);
            if (rangeInvalid.GetValueOrDefault(false))
            {
                throw new InvalidOperationException("Invalid range.");
            }
            bool? rangeContainsIndex = outer.NestedContains(index);
            if (!rangeContainsIndex.GetValueOrDefault(true))
            {
                throw new ArgumentException("Not contained within the range.", nameof(index));
            }

            if (index.IsFromEnd)
            {
                if (outer.End.IsFromEnd)
                {
                    return new Index(outer.End.Value + index.Value, /*fromEnd=*/true);
                }
                else
                {
                    return new Index(outer.End.Value - index.Value);
                }
            }
            else
            {
                if (outer.Start.IsFromEnd)
                {
                    return new Index(outer.Start.Value - index.Value, /*fromEnd=*/true);
                }
                else
                {
                    return new Index(outer.Start.Value + index.Value);
                }
            }
        }

        // Takes a Range `inner` nested within `this` (e.g. indexing so that offset 0 is `this.Start`), and
        // converts to an unnested Range (with the same offset space as `this`). Validates ranges are valid
        // and that `inner` is contained within `this` when able (but not always able).
        public static Range UnNest(this Range outer, Range inner)
        {
            bool? outerInvalid = outer.End.LessThan(outer.Start);
            if (outerInvalid.GetValueOrDefault(false))
            {
                throw new InvalidOperationException("Invalid range.");
            }
            bool? innerInvalid = outer.End.LessThan(outer.Start);
            if (innerInvalid.GetValueOrDefault(false))
            {
                throw new ArgumentException("Invalid range.", nameof(inner));
            }

            return new Range(outer.UnNest(inner.Start), outer.UnNest(inner.End));
        }

        // Parses a string of the form `[whitespace][^]Int32[whitespace]`, where `Int32` is parsed via the normal C#
        // `Int32.Parse(string)` logic.
        public static Index ParseIndex(string str)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            str = str.Trim();

            bool fromEnd = false;
            if (str.StartsWith('^'))
            {
                fromEnd = true;
                if (str.Length == 1)
                {
                    throw new FormatException("Could not parse index.");
                }
                str = str[1..];
            }

            return new Index(int.Parse(str), fromEnd);
        }
        public static bool TryParseIndex(string str, out Index parsed)
        {
            parsed = new Index();

            if (str is null)
            {
                return false;
            }

            bool fromEnd = false;
            if (str.StartsWith('^'))
            {
                fromEnd = true;
                if (str.Length == 1)
                {
                    return false;
                }
                str = str[1..];
            }

            int val;
            if (!int.TryParse(str, out val))
            {
                return false;
            }

            if (val < 0)
            {
                return false;
            }

            parsed = new Index(val, fromEnd);
            return true;
        }

        // Parses a string of the form `[whitespace]Index[..Index][whitespace]`, where `Index` is parsed via
        // ParseIndex(). If only a single Index is provided (no ".."), it is parsed as a Range of
        // `index..(index.Add(1))`. Validates the resulting range is valid when able (but not always able).
        public static Range Parse(string str)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            Match match = Regex.Match(str, "^([^.]*)?(\\.\\.([^.]*)?)?$");
            if (!match.Success ||
                string.IsNullOrEmpty(match.Groups[1].Value) &&
                string.IsNullOrEmpty(match.Groups[2].Value))
            {
                throw new FormatException("Could not parse range.");
            }

            Index i1;
            Index i2;
            if (string.IsNullOrEmpty(match.Groups[2].Value))
            {
                Debug.Assert(match.Groups[1].Success);
                Debug.Assert(!match.Groups[3].Success);

                i1 = ParseIndex(match.Groups[1].Value);
                i2 = i1.Add(1);
            }
            else
            {
                if (string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    i1 = 0;
                }
                else
                {
                    i1 = ParseIndex(match.Groups[1].Value);
                }

                if (string.IsNullOrEmpty(match.Groups[3].Value))
                {
                    i2 = ^0;
                }
                else
                {
                    i2 = ParseIndex(match.Groups[3].Value);
                }
            }


            if (!i1.LessThan(i2).GetValueOrDefault(true))
            {
                throw new ArgumentException("Parsed range invalid.");
            }

            return i1..i2;
        }
        public static bool TryParse(string str, out Range parsed)
        {
            parsed = new Range();

            if (str is null)
            {
                return false;
            }

            Match match = Regex.Match(str, "^([^.]*)?(\\.\\.([^.]*)?)?$");
            if (!match.Success ||
                string.IsNullOrEmpty(match.Groups[1].Value) &&
                string.IsNullOrEmpty(match.Groups[2].Value))
            {
                return false;
            }

            Index i1;
            Index i2;
            if (string.IsNullOrEmpty(match.Groups[2].Value))
            {
                Debug.Assert(match.Groups[1].Success);
                Debug.Assert(!match.Groups[3].Success);

                if (!TryParseIndex(match.Groups[1].Value, out i1))
                {
                    return false;
                }
                i2 = i1.Add(1);
            }
            else
            {
                if (string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    i1 = 0;
                }
                else if (!TryParseIndex(match.Groups[1].Value, out i1))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(match.Groups[3].Value))
                {
                    i2 = ^0;
                }
                else if (!TryParseIndex(match.Groups[3].Value, out i2))
                {
                    return false;
                }
            }

            if (!i1.LessThan(i2).GetValueOrDefault(true))
            {
                return false;
            }

            parsed = i1..i2;
            return true;
        }
    }
}
