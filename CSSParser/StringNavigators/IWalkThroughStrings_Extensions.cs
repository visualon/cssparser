using System;

namespace CSSParser.StringNavigators
{
	public static class IWalkThroughStrings_Extensions
	{
		/// <summary>
		/// This is a convenience method signature onto DoesCurrentContentMatch where a null optionalComparer is passed (meaning precise matching is required)
		/// </summary>
		public static bool DoesCurrentContentMatch(this IWalkThroughStrings source, string value)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			return source.DoesCurrentContentMatch(value, null);
		}
	}
}
