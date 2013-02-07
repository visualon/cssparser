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
