using System;

namespace CSSParser.ExtendedLESSParser
{
	public class StylePropertyValue : StyleProperty
	{
		public StylePropertyValue(StylePropertyName property, string value, int sourceLineIndex)
			: base(value, sourceLineIndex)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			Property = property;
		}

		/// <summary>
		/// This will never be null
		/// </summary>
		public StylePropertyName Property { get; private set; }
	}
}
