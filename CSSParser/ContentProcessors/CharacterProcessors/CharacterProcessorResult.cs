using System;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class CharacterProcessorResult
	{
		public CharacterProcessorResult(CharacterCategorisationOptions characterCategorisation, IProcessCharacters nextProcessor)
		{
			if (!Enum.IsDefined(typeof(CharacterCategorisationOptions), characterCategorisation))
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
