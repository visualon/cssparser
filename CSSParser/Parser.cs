using System;
using System.Collections.Generic;
using CSSParser.ContentProcessors.CharacterProcessors;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;
using CSSParser.ContentProcessors.StringProcessors;
using CSSParser.StringNavigators;

namespace CSSParser
{
	public static class Parser
	{
		/// <summary>
		/// This will never return null nor a set containing any null references. It will throw an exception for a null content reference.
		/// </summary>
		public static IEnumerable<CategorisedCharacterString> Parse(string content)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			var processorFactory = new CachingCharacterProcessorsFactory(
				new CharacterProcessorsFactory()
			);
			return (new ProcessedCharactersGrouper()).GetStrings(
				new StringNavigator(content),
				processorFactory.Get<SelectorOrStylePropertySegment>(processorFactory)
			);
		}
	}
}
