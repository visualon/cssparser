using System;
using CSSParser;
using CSSParser.ContentProcessors.StringProcessors;
using Xunit;

namespace UnitTests
{
    public class LessParserTests
    {
        [Fact]
        public void NullString()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Parser.ParseLESS((string)null);
            });
        }

        [Fact]
        public void BlankContent()
        {
            Assert.Equal(
                new CategorisedCharacterString[0],
                Parser.ParseLESS(""),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void EmptyBodyTagOnSingleLine()
        {
            var content = "body { }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("body", 0),
                CSS.Whitespace(" ", 4),
                CSS.OpenBrace(5),
                CSS.Whitespace(" ", 6),
                CSS.CloseBrace(7)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void ClassNameIsPartOfSelector()
        {
            var content = "p.note { font-style: italic }";
            var expected = new CategorisedCharacterString[]
			{
                CSS.SelectorOrStyleProperty("p.note", 0),
                CSS.Whitespace(" ", 6),
                CSS.OpenBrace(7),
                CSS.Whitespace(" ", 8),
                CSS.SelectorOrStyleProperty("font-style", 9),
                CSS.StylePropertyColon(19),
                CSS.Whitespace(" ", 20),
                CSS.Value("italic", 21),
                CSS.Whitespace(" ", 27),
                CSS.CloseBrace(28)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        /// <summary>
        /// Whitespace should be characters that can safely be removed from style content without affecting its meaning - this does not apply to whitespace
        /// within style values, since removing that whitespace may affect the meaning of the content. As such, whitespace that is part of a quoted value
        /// should be categorised as type Value and not as type Whitespace.
        /// </summary>
        [Fact]
        public void WhitespaceInQuotedValueIsCategorisedAsValueAndNotAsWhitespace()
        {
            var content = "body { font-family: \"Segoe UI\"; }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("body", 0),
                CSS.Whitespace(" ", 4),
                CSS.OpenBrace(5),
                CSS.Whitespace(" ", 6),
                CSS.SelectorOrStyleProperty("font-family", 7),
                CSS.StylePropertyColon(18),
                CSS.Whitespace(" ", 19),
                CSS.Value("\"Segoe UI\"", 20),
                CSS.SemiColon(30),
                CSS.Whitespace(" ", 31),
                CSS.CloseBrace(32)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void TerminatingLineReturnIsPartOfSingleLineComment()
        {
            var content = "// Comment\nbody { }";
            var expected = new CategorisedCharacterString[]
			{
                CSS.Comment("// Comment\n", 0),
                CSS.SelectorOrStyleProperty("body", 11),
                CSS.Whitespace(" ", 15),
                CSS.OpenBrace(16),
                CSS.Whitespace(" ", 17),
                CSS.CloseBrace(18)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void NonContainedLineReturnIsNotPartOfMultiLineComment()
        {
            var content = "/* Comment */\nbody { }";
            var expected = new CategorisedCharacterString[]
			{
                CSS.Comment("/* Comment */", 0),
                CSS.Whitespace("\n", 13),
                CSS.SelectorOrStyleProperty("body", 14),
                CSS.Whitespace(" ", 18),
                CSS.OpenBrace(19),
                CSS.Whitespace(" ", 20),
                CSS.CloseBrace(21)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void CommentedOutPropertyValue()
        {
            var content = "a { color: /*black*/ red; }";
            var expected = new CategorisedCharacterString[]
			{
                CSS.SelectorOrStyleProperty("a", 0),
                CSS.Whitespace(" ", 1),
                CSS.OpenBrace(2),
                CSS.Whitespace(" ", 3),
                CSS.SelectorOrStyleProperty("color", 4),
                CSS.StylePropertyColon(9),
                CSS.Whitespace(" ", 10),
                CSS.Comment("/*black*/", 11),
                CSS.Whitespace(" ", 20),
                CSS.Value("red", 21),
                CSS.SemiColon(24),
                CSS.Whitespace(" ", 25),
                CSS.CloseBrace(26)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void SingleLineCommentAnnotatedPropertyValue()
        {
            var content = "a {\n  color: black; // red\n}";
            var expected = new CategorisedCharacterString[]
			{
                CSS.SelectorOrStyleProperty("a", 0),
                CSS.Whitespace(" ", 1),
                CSS.OpenBrace(2),
                CSS.Whitespace("\n  ", 3),
                CSS.SelectorOrStyleProperty("color", 6),
                CSS.StylePropertyColon(11),
                CSS.Whitespace(" ", 12),
                CSS.Value("black", 13),
                CSS.SemiColon(18),
                CSS.Whitespace(" ", 19),
                CSS.Comment("// red\n", 20),
                CSS.CloseBrace(27)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        /// <summary>
        /// Most adjacent characters of the same type are combined into a single CategorisedCharacterString but for closing braces this feels wrong when
        /// the API is used - multiple closing braces do not represent a single "value", they are important individually (unlike the characters of an
        /// actual Style Property Value; those characters only make sense as part of a complete string)
        /// </summary>
        [Fact]
        public void ClosingBraceCharactersAreNotCombined()
        {
            var content = "p { a { color: black; }}";
            var expected = new CategorisedCharacterString[]
			{
                CSS.SelectorOrStyleProperty("p", 0),
                CSS.Whitespace(" ", 1),
                CSS.OpenBrace(2),
                CSS.Whitespace(" ", 3),
                CSS.SelectorOrStyleProperty("a", 4),
                CSS.Whitespace(" ", 5),
                CSS.OpenBrace(6),
                CSS.Whitespace(" ", 7),
                CSS.SelectorOrStyleProperty("color", 8),
                CSS.StylePropertyColon(13),
                CSS.Whitespace(" ", 14),
                CSS.Value("black", 15),
                CSS.SemiColon(20),
                CSS.Whitespace(" ", 21),
                CSS.CloseBrace(22),
                CSS.CloseBrace(23)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void MultipleSelectorsAppearAsMultipleStringsIfTheyAreSeparatedByWhitespace()
        {
            var content = "p, a { }";
            var expected = new CategorisedCharacterString[]
			{
                CSS.SelectorOrStyleProperty("p,", 0),
                CSS.Whitespace(" ", 2),
                CSS.SelectorOrStyleProperty("a", 3),
                CSS.Whitespace(" ", 4),
                CSS.OpenBrace(5),
                CSS.Whitespace(" ", 6),
                CSS.CloseBrace(7)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void MultipleSelectorsAppearAsSingleStringsIfNotSeparatedByWhitespace()
        {
            var content = "p,a { }";
            var expected = new CategorisedCharacterString[]
			{
                CSS.SelectorOrStyleProperty("p,a", 0),
                CSS.Whitespace(" ", 3),
                CSS.OpenBrace(4),
                CSS.Whitespace(" ", 5),
                CSS.CloseBrace(6)
			};
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }


        // TODO: Various comment arrangements (single line, multi line, fragment - eg. "colour: /*black*/ red;" or "color: black; // red")

        // TODO: Single property without semi-colon
        // TODO: Single property WITH semi-colon
        // TODO: Multiple properties
        // TODO: Nested
        // TODO: Nested plus multiple properties
        // TODO: Nested plus multiple properties before AND after\

        // TODO: class, id, multiple selectors, multiple classes (.c1.c2)
















        [Fact]
        public void PseudoClassesShouldNotBeIdentifiedAsPropertyValues()
        {
            var content = "a:hover { color: blue; }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("a:hover", 0),
                CSS.Whitespace(" ", 7),
                CSS.OpenBrace(8),
                CSS.Whitespace(" ", 9),
                CSS.SelectorOrStyleProperty("color", 10),
                CSS.StylePropertyColon(15),
                CSS.Whitespace(" ", 16),
                CSS.Value("blue", 17),
                CSS.SemiColon(21),
                CSS.Whitespace(" ", 22),
                CSS.CloseBrace(23)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void AttributeSelectorsShouldNotBeIdentifiedAsPropertyValues()
        {
            var content = "a[href] { }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("a[href]", 0),
                CSS.Whitespace(" ", 7),
                CSS.OpenBrace(8),
                CSS.Whitespace(" ", 9),
                CSS.CloseBrace(10)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void AttributeSelectorsWithQuotedContentShouldNotBeIdentifiedAsPropertyValues()
        {
            var content = "input[type=\"text\"] { color: blue; }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("input[type=\"text\"]", 0),
                CSS.Whitespace(" ", 18),
                CSS.OpenBrace(19),
                CSS.Whitespace(" ", 20),
                CSS.SelectorOrStyleProperty("color", 21),
                CSS.StylePropertyColon(26),
                CSS.Whitespace(" ", 27),
                CSS.Value("blue", 28),
                CSS.SemiColon(32),
                CSS.Whitespace(" ", 33),
                CSS.CloseBrace(34)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void LESSMixinArgumentDefaultsShouldNotBeIdentifiedAsPropertyValues()
        {
            var content = ".RoundedCorners (@radius: 4px) { border-radius: @radius; }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty(".RoundedCorners", 0),
                CSS.Whitespace(" ", 15),
                CSS.SelectorOrStyleProperty("(@radius: 4px)", 16),
                CSS.Whitespace(" ", 30),
                CSS.OpenBrace(31),
                CSS.Whitespace(" ", 32),
                CSS.SelectorOrStyleProperty("border-radius", 33),
                CSS.StylePropertyColon(46),
                CSS.Whitespace(" ", 47),
                CSS.Value("@radius", 48),
                CSS.SemiColon(55),
                CSS.Whitespace(" ", 56),
                CSS.CloseBrace(57)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void PseudoClassesShouldNotBeIdentifiedAsPropertyValuesWhenMinified()
        {
            var content = "a:hover{color:blue}";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("a:hover", 0),
                CSS.OpenBrace(7),
                CSS.SelectorOrStyleProperty("color", 8),
                CSS.StylePropertyColon(13),
                CSS.Value("blue", 14),
                CSS.CloseBrace(18)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void PseudoClassesShouldNotBeIdentifiedAsPropertyValuesWhenWhitespaceIsPresentAroundTheColon()
        {
            var content = "a : hover{}";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("a", 0),
                CSS.Whitespace(" ", 1),
                CSS.SelectorOrStyleProperty(":", 2),
                CSS.Whitespace(" ", 3),
                CSS.SelectorOrStyleProperty("hover", 4),
                CSS.OpenBrace(9),
                CSS.CloseBrace(10)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void EndOfQuotedStylePropertyMayNotBeEndOfEntryStyleProperty()
        {
            var content = "body { font-family: \"Segoe UI\", Verdana; }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("body", 0),
                CSS.Whitespace(" ", 4),
                CSS.OpenBrace(5),
                CSS.Whitespace(" ", 6),
                CSS.SelectorOrStyleProperty("font-family", 7),
                CSS.StylePropertyColon(18),
                CSS.Whitespace(" ", 19),
                CSS.Value("\"Segoe UI\",", 20),
                CSS.Whitespace(" ", 31),
                CSS.Value("Verdana", 32),
                CSS.SemiColon(39),
                CSS.Whitespace(" ", 40),
                CSS.CloseBrace(41)
            };

            Assert.Equal(
                expected,

                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

        [Fact]
        public void MediaQueryCriteriaShouldBeIdentifiedAsSelectorContent()
        {
            var content = "@media screen and (min-width: 600px) { body { background: white url(\"awesomecats.png\") no-repeat; } }";
            var expected = new CategorisedCharacterString[]
            {
                CSS.SelectorOrStyleProperty("@media", 0),
                CSS.Whitespace(" ", 6),
                CSS.SelectorOrStyleProperty("screen", 7),
                CSS.Whitespace(" ", 13),
                CSS.SelectorOrStyleProperty("and", 14),
                CSS.Whitespace(" ", 17),
                CSS.SelectorOrStyleProperty("(min-width:", 18),
                CSS.Whitespace(" ", 29),
                CSS.SelectorOrStyleProperty("600px)", 30),
                CSS.Whitespace(" ", 36),
                CSS.OpenBrace(37),
                CSS.Whitespace(" ", 38),
                CSS.SelectorOrStyleProperty("body", 39),
                CSS.Whitespace(" ", 43),
                CSS.OpenBrace(44),
                CSS.Whitespace(" ", 45),
                CSS.SelectorOrStyleProperty("background", 46),
                CSS.StylePropertyColon(56),
                CSS.Whitespace(" ", 57),
                CSS.Value("white", 58),
                CSS.Whitespace(" ", 63),
                CSS.Value("url(\"awesomecats.png\")", 64),
                CSS.Whitespace(" ", 86),
                CSS.Value("no-repeat", 87),
                CSS.SemiColon(96),
                CSS.Whitespace(" ", 97),
                CSS.CloseBrace(98),
                CSS.Whitespace(" ", 99),
                CSS.CloseBrace(100)
            };
            Assert.Equal(
                expected,
                Parser.ParseLESS(content),
                new CategorisedCharacterStringComparer()
            );
        }

    }
}
