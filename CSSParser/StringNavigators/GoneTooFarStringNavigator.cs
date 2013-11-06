using System;
using System.Collections.Generic;

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
		/// This will return true if the content is at least as long as the specified value string and if the next n characters (where n is the length of
		/// the value string) correspond to each of the value string's characters. This testing will be done according to the optionalComparer if non-null
		/// and will apply a simple char comparison (precise) match if a null optionalComparer is specified. An exception will be raised for a null or
		/// blank value. If there is insufficient content available to match the length of the value argument then false will be returned.
		/// </summary>
		public bool DoesCurrentContentMatch(string value, IEqualityComparer<char> optionalComparer)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException("Null/blank value specified");

			return false;
		}
	}
}
