namespace CSSParser.ContentProcessors
{
	public enum CharacterCategorisationOptions
	{
		Comment,
		CloseBrace,
		OpenBrace,
		SemiColon,
		SelectorOrStyleProperty,
		
		/// <summary>
		/// This is the colon between a Style Property and Value (not any colons that may exist in a media query, for example)
		/// </summary>
		StylePropertyColon,
		
		Value,
		Whitespace
	}
}
