using System;
using CSSParser.StringNavigators;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class QuotedValueSegment : IProcessCharacters
	{
		private readonly char _quoteCharacter;
		private readonly IProcessCharacters _characterProcessorToReturnTo;
		private readonly IGenerateCharacterProcessors _processorFactory;
		public QuotedValueSegment(char quoteCharacter, IProcessCharacters characterProcessorToReturnTo, IGenerateCharacterProcessors processorFactory)
		{
			if (characterProcessorToReturnTo == null)
				throw new ArgumentNullException("characterProcessorToReturnTo");
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");

			_quoteCharacter = quoteCharacter;
			_characterProcessorToReturnTo = characterProcessorToReturnTo;
			_processorFactory = processorFactory;
		}

		public CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			// If the next character is a backslash then the next character should be ignored if it's "special" and just considered
			// to be another character in the Value string (particularly important if the next character is an escaped quote)
			if (stringNavigator.CurrentCharacter == '\\')
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Value,
					_processorFactory.Get<SkipCharactersSegment>(
						CharacterCategorisationOptions.Value,
						1,
						this
					)
				);
			}

			// If this is the closing quote character then include it in the Value and then return to the previous processor
			if (stringNavigator.CurrentCharacter == _quoteCharacter)
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Value,
					_characterProcessorToReturnTo
				);
			}

			return new CharacterProcessorResult(
				CharacterCategorisationOptions.Value,
				this
			);
		}
	}
}
