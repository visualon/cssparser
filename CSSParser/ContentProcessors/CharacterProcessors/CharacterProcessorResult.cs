using System;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class CharacterProcessorResult
	{
		public CharacterProcessorResult(CharacterCategorisationOptions characterCategorisation, IProcessCharacters nextProcessor)
		{
			if ((characterCategorisation != CharacterCategorisationOptions.CloseBrace)
			&& (characterCategorisation != CharacterCategorisationOptions.Comment)
			&& (characterCategorisation != CharacterCategorisationOptions.OpenBrace)
			&& (characterCategorisation != CharacterCategorisationOptions.SemiColon)
			&& (characterCategorisation != CharacterCategorisationOptions.SelectorOrStyleProperty)
			&& (characterCategorisation != CharacterCategorisationOptions.StylePropertyColon)
			&& (characterCategorisation != CharacterCategorisationOptions.Value)
			&& (characterCategorisation != CharacterCategorisationOptions.Whitespace))
				throw new ArgumentOutOfRangeException("characterCategorisation");
			if (nextProcessor == null)
				throw new ArgumentNullException("nextProcessor");

			CharacterCategorisation = characterCategorisation;
			NextProcessor = nextProcessor;
		}

		public CharacterCategorisationOptions CharacterCategorisation { get; private set; }

		/// <summary>
		/// This will never be null
		/// </summary>
		public IProcessCharacters NextProcessor { get; private set; }
	}
}
