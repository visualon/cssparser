using System;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class SkipCharactersSegment : IProcessCharacters
	{
		private readonly IProcessCharacters _characterProcessorToReturnTo;
		public SkipCharactersSegment(
			CharacterCategorisationOptions characterCategorisation,
			int numberOfCharactersToSkip,
			IProcessCharacters characterProcessorToReturnTo)
		{
			if (!Enum.IsDefined(typeof(CharacterCategorisationOptions), characterCategorisation))
				throw new ArgumentOutOfRangeException("characterCategorisation");
			if (numberOfCharactersToSkip <= 0)
				throw new ArgumentOutOfRangeException("numberOfCharactersToSkip", "must be greater than zero");
			if (characterProcessorToReturnTo == null)
				throw new ArgumentNullException("characterProcessorToReturnTo");

			CharacterCategorisation = characterCategorisation;
			NumberOfCharactersToSkip = numberOfCharactersToSkip;
			_characterProcessorToReturnTo = characterProcessorToReturnTo;
		}

		public CharacterCategorisationOptions CharacterCategorisation { get; private set; }

		public int NumberOfCharactersToSkip { get; private set; }

		public CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			if (NumberOfCharactersToSkip == 1)
			{
				return new CharacterProcessorResult(
					CharacterCategorisation,
					_characterProcessorToReturnTo
				);
			}

			return new CharacterProcessorResult(
				CharacterCategorisation,
				new SkipCharactersSegment(CharacterCategorisation, NumberOfCharactersToSkip - 1, _characterProcessorToReturnTo)
			);
		}
	}
}
