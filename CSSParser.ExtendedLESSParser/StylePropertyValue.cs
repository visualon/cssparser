using System;
using System.Collections.Generic;
using System.Linq;

namespace CSSParser.ExtendedLESSParser
{
	public class StylePropertyValue : ICSSFragment
	{
		public StylePropertyValue(StylePropertyName property, IEnumerable<string> valueSegments, int sourceLineIndex)
		{
			if (property == null)
				throw new ArgumentNullException("property");
			if (valueSegments == null)
				throw new ArgumentNullException("valueSegments");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");

			ValueSegments = valueSegments.Select(v => (v ?? "").Trim()).ToList().AsReadOnly();
			if (ValueSegments.Any(v => v == null))
				throw new ArgumentException("Null/blank reference encountered in valueSegments set");

			Property = property;
			SourceLineIndex = sourceLineIndex;
		}

		/// <summary>
		/// This will never be null
		/// </summary>
		public StylePropertyName Property { get; private set; }

		/// <summary>
		/// This will never be null empty or contain any null or blank values, all of the strings will have been trimmed
		/// </summary>
		public IEnumerable<string> ValueSegments { get; private set; }

		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		public int SourceLineIndex { get; private set; }

		public override string ToString()
		{
			return base.ToString() + ":" + string.Join(" ", ValueSegments);
		}
	}
}
