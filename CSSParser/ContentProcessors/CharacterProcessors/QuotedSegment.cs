using System;
using CSSParser.StringNavigators;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	/// <summary>
	/// This may be a quoted section of a property value or of an attribute selector (this will be determined by the characterCategorisation
	/// passed to the constructor)
	/// </summary>
	public class QuotedSegment : IProcessCharacters
	{
		private readonly char _quoteCharacter;
		private readonly CharacterCategorisationOptions _characterCategorisation;
		private readonly IProcessCharacters _characterProcessorToReturnTo;
		private readonly IGenerateCharacterProcessors _processorFactory;
		public QuotedSegment(
			char quoteCharacter,
			CharacterCategorisationOptions characterCategorisation,
			IProcessCharacters characterProcessorToReturnTo,
			IGenerateCharacterProcessors processorFactory)
		{
			if (characterProcessorToReturnTo == null)
				throw new ArgumentNullException("characterProcessorToReturnTo");
			if (!Enum.IsDefined(typeof(CharacterCategorisationOptions), characterCategorisation))
				throw new ArgumentOutOfRangeException("characterCategorisation");
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");

			_quoteCharacter = quoteCharacter;
			_characterCategorisation = characterCategorisation;
			_characterProcessorToReturnTo = characterProcessorToReturnTo;
			_processorFactory = processorFactory;
		}

		public CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			// If the next character is a backslash then the next character should be ignored if it's "special" and just considered
			// to be another character in the Value string (particularly important if the next character is an escaped quote)
			var currentCharacter = stringNavigator.CurrentCharacter;
			if (currentCharacter == '\\')
			{
				return new CharacterProcessorResult(
					_characterCategorisation,
					_processorFactory.Get<SkipCharactersSegment>(
						_characterCategorisation,
						1,
						this
					)
				);
			}

			// If this is the closing quote character then include it in the Value and then return to the previous processor
			if (currentCharacter == _quoteCharacter)
			{
				return new CharacterProcessorResult(
					_characterCategorisation,
					_characterProcessorToReturnTo
				);
			}

			return new CharacterProcessorResult(
				_characterCategorisation,
				this
			);
		}
	}
}
