using System;

namespace CSSParser.ExtendedLESSParser.ContentSections
{
	/// <summary>
	/// This ContainerFragment represent a Media Query, its Selectors SelectorSet should only have a single value (unlike the Selector class which may have multiple
	/// Selectors - eg. "div#Header h2, div#Footer"). LESS CSS supports Media Queries that wrap styles and/or other selectors whilst regular CSS only supports the
	/// wrapping of selectors in Media Queries.
	/// </summary>
	public class Import : ICSSFragment
	{
		public Import(string importContent, int sourceLineIndex)
		{
			if (string.IsNullOrWhiteSpace(importContent))
				throw new ArgumentException("Null/blank importContent specified");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");

			ImportContent = importContent.Trim();
			SourceLineIndex = sourceLineIndex;
		}
	
		/// <summary>
		/// This will never be null or blank
		/// </summary>
		public string ImportContent { get; private set; }

		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		public int SourceLineIndex { get; private set; }
	}
}
