using System;

namespace CSSParser.ExtendedLESSParser.ContentSections
{
	public class StylePropertyName : ICSSFragment
	{
		public StylePropertyName(string value, int sourceLineIndex)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException("Null/blank value specified");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");

			Value = value.Trim();
			SourceLineIndex = sourceLineIndex;
		}

		/// <summary>
		/// This will never be null or blank, its value will always be trimmed
		/// </summary>
		public string Value { get; private set; }

		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		public int SourceLineIndex { get; private set; }

		public override string ToString()
		{
			return base.ToString() + ":" + Value;
		}
	}
}
