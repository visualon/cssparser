namespace CSSParser.ExtendedLESSParser.ContentSections
{
	/// <summary>
	/// This represents a significant CSS (or LESS) fragment such as selectors (which include media queries as well as CSS selectors) and style property names and values.
	/// There will not be any fragments for whitespace or comments, nor break characters such as colons or semicolons.
	/// </summary>
	public interface ICSSFragment
	{
		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		int SourceLineIndex { get; }
	}
}
