using System;
using System.Collections.Generic;
using System.Text;
using CSSParser.ContentProcessors.CharacterProcessors;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.StringProcessors
{
	public class ProcessedCharactersGrouper : ICollectStringsOfProcessedCharacters
	{
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
			var processedStrings = new List<CategorisedCharacterString>();
			var stringBuilder = new StringBuilder();
			while (contentWalker.CurrentCharacter != null)
			{
				var processResult = contentProcessor.Process(contentWalker);
				if (processResult.CharacterCategorisation != currentCharacterType)
				{
					if (stringBuilder.Length > 0)
					{
						processedStrings.Add(new CategorisedCharacterString(stringBuilder.ToString(), currentCharacterType));
						stringBuilder.Clear();
					}
					currentCharacterType = processResult.CharacterCategorisation;
				}
				stringBuilder.Append(contentWalker.CurrentCharacter);

				contentProcessor = processResult.NextProcessor;
				contentWalker = contentWalker.Next;
			}
			if (stringBuilder.Length > 0)
				processedStrings.Add(new CategorisedCharacterString(stringBuilder.ToString(), currentCharacterType));

			return processedStrings;
		}
	}
}
