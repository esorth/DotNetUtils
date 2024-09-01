// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2024 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

namespace DotNetUtils
{
    public interface IIntervalTree<TValue> : IDictionary<Range, TValue>,
                                             IReadOnlyIntervalTree<TValue>
    {
    }

    public interface IReadOnlyIntervalTree<TValue> : IReadOnlyDictionary<Range, TValue>
    {
        // Gets an interval tree of all tree contents that are known to overlap with `idx`. Will not
        // return any entries whose key's beginning or end has a different `IsFromEnd` property than
        // `idx` because the tree cannot determine whether or not it overlaps.
        public IIntervalTree<TValue> GetOverlap(Index idx);

        // Gets an interval tree of all tree contents that are known to overlap with `range`. Will
        // not return any entries whose key's beginning and end have different `IsFromEnd`
        // properties than the respective beginning and end from `range` because the tree cannot
        // determine whether or not it overlaps.
        public IIntervalTree<TValue> GetOverlap(Range range);

        // Alternate versions of `GetOverlap()` that return immutable interval trees.
        public IImmutableIntervalTree<TValue> GetImmutableOverlap(Index idx);
        public IImmutableIntervalTree<TValue> GetImmutableOverlap(Range range);
    }

    public interface IImmutableIntervalTree<TValue> : IReadOnlyIntervalTree<TValue>
    { }
}
