using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSSParser.ContentProcessors.CharacterProcessors;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.StringProcessors
{
	/// <summary>
	/// This will use IWalkThroughStrings and IProcessCharacters to generate a set of CategorisedCharacterString instances. This implementation will only parse the
	/// content while the returned set is being enumerated (so if only the start of the content is being examined then the work to parse the rest of the content
	/// need not be performed). Most runs of characters that are of the same CharacterCategorisationOptions will be combined into one string - the only types
	/// that will not be combined are CloseBrace, OpenBrace and SemiColon since it seems counter-intuitive when using the parsed content for this to be
	/// the case (eg. when looking for two closing braces, it makes more sense to expect two individual strings of type CloseBrace than one string
	/// of type CloseBrace that is two characters long).
	/// </summary>
	public class ProcessedCharactersGrouper : ICollectStringsOfProcessedCharacters
	{
		private static CharacterCategorisationOptions[] CharacterTypesToNotGroup = new[]
		{
			CharacterCategorisationOptions.CloseBrace,
			CharacterCategorisationOptions.OpenBrace,
			CharacterCategorisationOptions.SemiColon
		};

		/// <summary>
		/// This will never return null nor a set containing any null references. It will throw an exception for null contentWalker or contentProcessor
		/// references or it the processing failed.
		/// </summary>
		public IEnumerable<CategorisedCharacterString> GetStrings(IWalkThroughStrings contentWalker, IProcessCharacters contentProcessor)
		{
			if (contentWalker == null)
				throw new ArgumentNullException("contentWalker");
			if (contentProcessor == null)
				throw new ArgumentNullException("contentProcessor");

			// It doesn'actually matter what the initial value of currentCharacterType is since it won't be used until there is string content
			// to record, and by that point it will have been assigned a value
			var currentCharacterType = CharacterCategorisationOptions.SelectorOrStyleProperty;
			var stringBuilder = new StringBuilder();
			var currentCharacterIndex = 0;
			while (contentWalker.CurrentCharacter != null)
			{
				var processResult = contentProcessor.Process(contentWalker);
				if ((processResult.CharacterCategorisation != currentCharacterType) || CharacterTypesToNotGroup.Contains(processResult.CharacterCategorisation))
				{
					if (stringBuilder.Length > 0)
					{
						var value = stringBuilder.ToString();
						yield return new CategorisedCharacterString(value, currentCharacterIndex - value.Length, currentCharacterType);
						stringBuilder.Clear();
					}
					currentCharacterType = processResult.CharacterCategorisation;
				}
				stringBuilder.Append(contentWalker.CurrentCharacter);

				contentProcessor = processResult.NextProcessor;
				contentWalker = contentWalker.Next;
				currentCharacterIndex++;
			}
			if (stringBuilder.Length > 0)
			{
				var value = stringBuilder.ToString();
				yield return new CategorisedCharacterString(value, currentCharacterIndex - value.Length, currentCharacterType);
			}
		}
	}
}
