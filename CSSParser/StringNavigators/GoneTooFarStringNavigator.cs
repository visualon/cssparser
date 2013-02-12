using System;

namespace CSSParser.StringNavigators
{
	public class GoneTooFarStringNavigator : IWalkThroughStrings
	{
		/// <summary>
		/// This return null if the current location in the string has no content (eg. anywhere on an empty string or past the end of a non-empty string)
		/// </summary>
		public char? CurrentCharacter { get { return null; } }

		/// <summary>
		/// This return null if the current location in the string has no content (eg. anywhere on an empty string or past the end of a non-empty string)
		/// </summary>
		public IWalkThroughStrings Next
		{
			get { return this; }
		}

		/// <summary>
		/// This will try to extract a string of length requiredNumberOfCharacters from the current position in the string navigator. If there are insufficient
		/// characters available, then a string containing all of the remaining characters will be returned. This will be an empty string if there is no more
		/// content to deliver. This will never return null.
		/// </summary>
		public string TryToGetCharacterString(int requiredNumberOfCharacters)
		{
			if (requiredNumberOfCharacters <= 0)
				throw new ArgumentOutOfRangeException("requiredNumberOfCharacters", "must be greater than zero");

			return "";
		}
	}
}
