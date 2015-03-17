using System;
using System.Collections.Generic;
using CSSParser.ContentProcessors.StringProcessors;

namespace UnitTests
{
    public class CategorisedCharacterStringComparer : IEqualityComparer<CategorisedCharacterString>
    {
        public bool Equals(CategorisedCharacterString x, CategorisedCharacterString y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");

            var match = 
                (x.CharacterCategorisation == y.CharacterCategorisation) &&
                (x.IndexInSource == y.IndexInSource) &&
                (x.Value == y.Value);
            if (!match)
            {
                // TODO: Remove
            }
            return match;
        }

        public int GetHashCode(CategorisedCharacterString obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            // This is irrelevant for our purposes, so returning zero for everything is fine (it's the Equals method that's important)
            return 0;
        }
    }
}
