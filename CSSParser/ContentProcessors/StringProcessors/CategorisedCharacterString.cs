using System;

namespace CSSParser.ContentProcessors.StringProcessors
{
	public class CategorisedCharacterString
	{
		public CategorisedCharacterString(string value, CharacterCategorisationOptions characterCategorisation)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException("Null/blank value specified");
			if (!Enum.IsDefined(typeof(CharacterCategorisationOptions), characterCategorisation))
				throw new ArgumentOutOfRangeException("characterCategorisation");

			Value = value;
			CharacterCategorisation = characterCategorisation;
		}

		/// <summary>
		/// This will never be null or an empty string
		/// </summary>
		public string Value { get; private set; }

		public CharacterCategorisationOptions CharacterCategorisation { get; private set; }
	}
}
