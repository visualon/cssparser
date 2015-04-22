using System;
using System.Collections.Generic;
using System.Linq;
using CSSParser;
using CSSParser.ContentProcessors;
using CSSParser.ContentProcessors.StringProcessors;
using Xunit;

namespace UnitTests
{
	/// <summary>
	/// These are closer to integration tests since they don't really target specific components of the parsing chain, but they're a start!
	/// </summary>
	public class ParserTests
	{
		[Fact]
		public void PseudoClassesShouldNotBeIdentifiedAsPropertyValues()
		{
			var content = "a:hover { color: blue; }";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString("a:hover", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 7, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("{", 8, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString(" ", 9, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("color", 10, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(":", 15, CharacterCategorisationOptions.StylePropertyColon),
				new CategorisedCharacterString(" ", 16, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("blue", 17, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(";", 21, CharacterCategorisationOptions.SemiColon),
				new CategorisedCharacterString(" ", 22, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("}", 23, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

		[Fact]
		public void AttributeSelectorsShouldNotBeIdentifiedAsPropertyValues()
		{
			var content = "a[href] { }";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString("a[href]", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 7, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("{", 8, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString(" ", 9, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("}", 10, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

		[Fact]
		public void AttributeSelectorsWithQuotedContentShouldNotBeIdentifiedAsPropertyValues()
		{
			var content = "input[type=\"text\"] { color: blue; }";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString("input[type=\"text\"]", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 18, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("{", 19, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString(" ", 20, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("color", 21, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(":", 26, CharacterCategorisationOptions.StylePropertyColon),
				new CategorisedCharacterString(" ", 27, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("blue", 28, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(";", 32, CharacterCategorisationOptions.SemiColon),
				new CategorisedCharacterString(" ", 33, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("}", 34, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

		[Fact]
		public void LESSMixinArgumentDefaultsShouldNotBeIdentifiedAsPropertyValues()
		{
			var content = ".RoundedCorners (@radius: 4px) { border-radius: @radius; }";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString(".RoundedCorners", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 15, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("(@radius: 4px)", 16, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 30, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("{", 31, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString(" ", 32, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("border-radius", 33, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(":", 46, CharacterCategorisationOptions.StylePropertyColon),
				new CategorisedCharacterString(" ", 47, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("@radius", 48, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(";", 55, CharacterCategorisationOptions.SemiColon),
				new CategorisedCharacterString(" ", 56, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("}", 57, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

		[Fact]
		public void PseudoClassesShouldNotBeIdentifiedAsPropertyValuesWhenMinified()
		{
			var content = "a:hover{color:blue}";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString("a:hover", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString("{", 7, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString("color", 8, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(":", 13, CharacterCategorisationOptions.StylePropertyColon),
				new CategorisedCharacterString("blue", 14, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString("}", 18, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

		[Fact]
		public void PseudoClassesShouldNotBeIdentifiedAsPropertyValuesWhenWhitespaceIsPresentAroundTheColon()
		{
			var content = "a : hover{}";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString("a", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 1, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString(":", 2, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 3, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("hover", 4, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString("{", 9, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString("}", 10, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

		[Fact]
		public void EndOfQuotedStylePropertyMayNotBeEndOfEntryStyleProperty()
		{
			var content = "body { font-family: \"Segoe UI\", Verdana; }";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString("body", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 4, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("{", 5, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString(" ", 6, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("font-family", 7, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(":", 18, CharacterCategorisationOptions.StylePropertyColon),
				new CategorisedCharacterString(" ", 19, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("\"Segoe UI\",", 20, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(" ", 31, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("Verdana", 32, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(";", 39, CharacterCategorisationOptions.SemiColon),
				new CategorisedCharacterString(" ", 40, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("}", 41, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

		[Fact]
		public void MediaQueryCriteriaShouldBeIdentifiedAsSelectorContent()
		{
			var content = "@media screen and (min-width: 600px) { body { background: white url(\"awesomecats.png\") no-repeat; } }";
			var expected = new CategorisedCharacterString[]
			{
				new CategorisedCharacterString("@media", 0, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 6, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("screen", 7, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 13, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("and", 14, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 17, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("(min-width:", 18, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 29, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("600px)", 30, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 36, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("{", 37, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString(" ", 38, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("body", 39, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(" ", 43, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("{", 44, CharacterCategorisationOptions.OpenBrace),
				new CategorisedCharacterString(" ", 45, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("background", 46, CharacterCategorisationOptions.SelectorOrStyleProperty),
				new CategorisedCharacterString(":", 56, CharacterCategorisationOptions.StylePropertyColon),
				new CategorisedCharacterString(" ", 57, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("white", 58, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(" ", 63, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("url(\"awesomecats.png\")", 64, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(" ", 86, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("no-repeat", 87, CharacterCategorisationOptions.Value),
				new CategorisedCharacterString(";", 96, CharacterCategorisationOptions.SemiColon),
				new CategorisedCharacterString(" ", 97, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("}", 98, CharacterCategorisationOptions.CloseBrace),
				new CategorisedCharacterString(" ", 99, CharacterCategorisationOptions.Whitespace),
				new CategorisedCharacterString("}", 100, CharacterCategorisationOptions.CloseBrace)
			};
			Assert.Equal<IEnumerable<CategorisedCharacterString>>(
				expected,
				Parser.ParseLESS(content),
				new ParsedContentComparer()
			);
		}

        

		private class ParsedContentComparer : IEqualityComparer<IEnumerable<CategorisedCharacterString>>
		{
			public bool Equals(IEnumerable<CategorisedCharacterString> x, IEnumerable<CategorisedCharacterString> y)
			{
				if (x == null)
					throw new ArgumentNullException("x");
				if (y == null)
					throw new ArgumentNullException("y");

				var xArray = x.ToArray();
				var yArray = y.ToArray();
				if (xArray.Length != yArray.Length)
					return false;

				for (var index = 0; index < xArray.Length; index++)
				{
					if ((xArray[index].CharacterCategorisation != yArray[index].CharacterCategorisation)
					|| (xArray[index].IndexInSource != yArray[index].IndexInSource)
					|| (xArray[index].Value != yArray[index].Value))
						return false;
				}
				return true;
			}

			public int GetHashCode(IEnumerable<CategorisedCharacterString> obj)
			{
				if (obj == null)
					throw new ArgumentNullException("obj");

				// This is irrelevant for our purposes, so returning zero for everything is fine (it's the Equals method that's important)
				return 0;
			}
		}
	}
}
