using CSSParser.ContentProcessors;
using CSSParser.ContentProcessors.StringProcessors;

namespace UnitTests
{
    /// <summary>
    /// These are helper functions to make the tests a little more concise
    /// </summary>
    public static class CSS
    {
        public static CategorisedCharacterString Comment(string value, int lineIndex)
        {
            return new CategorisedCharacterString(value, lineIndex, CharacterCategorisationOptions.Comment);
        }
        public static CategorisedCharacterString CloseBrace(int lineIndex)
        {
            return new CategorisedCharacterString("}", lineIndex, CharacterCategorisationOptions.CloseBrace);
        }
        public static CategorisedCharacterString OpenBrace(int lineIndex)
        {
            return new CategorisedCharacterString("{", lineIndex, CharacterCategorisationOptions.OpenBrace);
        }
        public static CategorisedCharacterString SelectorOrStyleProperty(string value, int lineIndex)
        {
            return new CategorisedCharacterString(value, lineIndex, CharacterCategorisationOptions.SelectorOrStyleProperty);
        }
        public static CategorisedCharacterString SemiColon(int lineIndex)
        {
            return new CategorisedCharacterString(";", lineIndex, CharacterCategorisationOptions.SemiColon);
        }
        public static CategorisedCharacterString StylePropertyColon(int lineIndex)
        {
            return new CategorisedCharacterString(":", lineIndex, CharacterCategorisationOptions.StylePropertyColon);
        }
        public static CategorisedCharacterString Value(string value, int lineIndex)
        {
            return new CategorisedCharacterString(value, lineIndex, CharacterCategorisationOptions.Value);
        }
        public static CategorisedCharacterString Whitespace(string value, int lineIndex)
        {
            return new CategorisedCharacterString(value, lineIndex, CharacterCategorisationOptions.Whitespace);
        }
    }
}
