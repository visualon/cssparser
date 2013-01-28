using System;
using System.Text;

namespace CSSParser.StringNavigators
{
	public static class IWalkThroughStrings_Extensions
	{
		/// <summary>
		/// This will try to extract a string of length requiredNumberOfCharacters from the current position in the string navigator. If there are insufficient
		/// characters available, then null will be returned. This will never return an empty string as requiredNumberOfCharacters must be a positive value.
		/// </summary>
		public static string TryToGetCharacterString(this IWalkThroughStrings stringNavigator, int requiredNumberOfCharacters)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");
			if (requiredNumberOfCharacters <= 0)
				throw new ArgumentOutOfRangeException("requiredNumberOfCharacters");

			var stringBuilder = new StringBuilder(requiredNumberOfCharacters);
			for (var index = 0; index < requiredNumberOfCharacters; index++)
			{
				if (stringNavigator.CurrentCharacter == null)
					return null;
				
				stringBuilder.Append(stringNavigator.CurrentCharacter.Value);
				stringNavigator = stringNavigator.Next;
			}
			return stringBuilder.ToString();
		}
	}
}
