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
	}
}
