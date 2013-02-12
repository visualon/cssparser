namespace CSSParser.StringNavigators
{
	public interface IWalkThroughStrings
	{
		/// <summary>
		/// This return null if the current location in the string has no content (eg. anywhere on an empty string or past the end of a non-empty string)
		/// </summary>
		char? CurrentCharacter { get; }

		/// <summary>
		/// This will never return null
		/// </summary>
		IWalkThroughStrings Next { get; }

		/// <summary>
		/// This will try to extract a string of length requiredNumberOfCharacters from the current position in the string navigator. If there are insufficient
		/// characters available, then a string containing all of the remaining characters will be returned. This will be an empty string if there is no more
		/// content to deliver. This will never return null.
		/// </summary>
		string TryToGetCharacterString(int requiredNumberOfCharacters);
	}
}
