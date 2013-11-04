using System;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	/// <summary>
	/// Once we're in a media query section, we don't leave it until we hit the open brace of the section it refers to (so don't, for example, allow any colons
	/// to be identified as StylePropertyColon when they are part of the media query as so should be marked as being a SelectorOrStyleProperty)
	/// </summary>
	public class MediaQuerySegment : IProcessCharacters
	{
		private readonly IProcessCharacters _characterProcessorToReturnTo;
		public MediaQuerySegment(IProcessCharacters characterProcessorToReturnTo)
		{
			if (characterProcessorToReturnTo == null)
				throw new ArgumentNullException("characterProcessorToReturnTo");

			_characterProcessorToReturnTo = characterProcessorToReturnTo;
		}

		public CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			var currentCharacter = stringNavigator.CurrentCharacter;
			if (currentCharacter == '{')
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.OpenBrace,
					_characterProcessorToReturnTo
				);
			}
			else if ((currentCharacter != null) && char.IsWhiteSpace(currentCharacter.Value))
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Whitespace,
					this
				);
			}

			return new CharacterProcessorResult(
				CharacterCategorisationOptions.SelectorOrStyleProperty,
				this
			);
		}
	}
}
