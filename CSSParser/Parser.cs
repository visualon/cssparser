using System;
using System.Collections.Generic;
using CSSParser.ContentProcessors.CharacterProcessors;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;
using CSSParser.ContentProcessors.StringProcessors;
using CSSParser.StringNavigators;

namespace CSSParser
{
	/// <summary>
	/// The parsing performed by these methods is intended to be as cheap as possible but in exchange it performs only simplistic parsing of the
	/// content (it doesn't differentiate between selectors - eg. "div.Header h2.Name, div.Footer" and style property names - eg. "color" - for
	/// example). The quality of the data may be improved by passing the returned CategorisedCharacterString set through the
	/// LessCssHierarchicalParser, this WILL differentiate between selectors and style property names and will represent nested styles (for
	/// LESS CSS content) and styles nested within media queries. It requires an additional processing step, though.
	/// </summary>
	public static class Parser
	{
		/// <summary>
		/// This will never return null nor a set containing any null references. It will throw an exception for a null content reference.
		/// CSS does not support single line comment, unlike LESS CSS. The content parsing is deferred so that the work to parse the content
		/// is only performed as the returned data is enumerated over. All runs of characters that are of the same CharacterCategorisationOptions
		/// will be combined into one string (note: this means that runs of opening braces that aren't separated by whitespace will be combined
		/// into one string containing those multiple braces).
		/// </summary>
		public static IEnumerable<CategorisedCharacterString> ParseCSS(string content)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			return Parse(content, false);
		}

		/// <summary>
		/// This will never return null nor a set containing any null references. It will throw an exception for a null content reference.
		/// LESS CSS supports single line comments as well the multiline comment format supported by standard CSS. The content parsing is
		/// deferred so that the work to parse the content is only performed as the returned data is enumerated over. All runs of characters
		/// that are of the same CharacterCategorisationOptions will be combined into one string (note: this means that runs of opening braces
		/// that aren't separated by whitespace will be combined into one string containing those multiple braces).
		/// </summary>
		public static IEnumerable<CategorisedCharacterString> ParseLESS(string content)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			return Parse(content, true);
		}

		private static IEnumerable<CategorisedCharacterString> Parse(string content, bool supportSingleLineComments)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			var processorFactory = new CachingCharacterProcessorsFactory(
				new CharacterProcessorsFactory()
			);
			return (new ProcessedCharactersGrouper()).GetStrings(
				(content == "") ? (IWalkThroughStrings)(new GoneTooFarStringNavigator()) : new StringNavigator(content),
				processorFactory.Get<SelectorOrStylePropertySegment>(
					supportSingleLineComments
						? SelectorOrStyleSegment.SingleLineCommentsSupportOptions.Support
						: SelectorOrStyleSegment.SingleLineCommentsSupportOptions.DoNotSupport,
					processorFactory
				)
			);
		}
	}
}
