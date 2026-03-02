// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2026 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

// ICollection equivalent of the various immutable interfaces in System.Collections.Immutable.
// Implementing classes must not allow modification, thus allowing any usage of this interface to
// safely assume immutability of the collection.

namespace DotNetUtils
{
    public interface IImmutableCollection<T> : IReadOnlyCollection<T>
    {
        public IImmutableCollection<T> Add(T item);
        public IImmutableCollection<T> Clear();
        public IImmutableCollection<T> Remove(T item);
    }
}
