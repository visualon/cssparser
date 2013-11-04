using System;

namespace CSSParser.StringNavigators
{
	public class StringNavigator : IWalkThroughStrings
	{
		private readonly char[] _value;
		private readonly int _index;
		private StringNavigator(char[] value, int index)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			if ((index < 0) || (index >= value.Length))
				throw new ArgumentOutOfRangeException("index");

			_value = value;
			_index = index;
			CurrentCharacter = _value[_index];
		}
		public StringNavigator(string value) : this((value != null) ? value.ToCharArray() : null, 0) { }

		/// <summary>
		/// This return null if the current location in the string has no content (eg. anywhere on an empty string or past the end of a non-empty string)
		/// </summary>
		public char? CurrentCharacter { get; private set; }

		/// <summary>
		/// This return null if the current location in the string has no content (eg. anywhere on an empty string or past the end of a non-empty string)
		/// </summary>
		public IWalkThroughStrings Next
		{
			get
			{
				if (_index == _value.Length - 1)
					return new GoneTooFarStringNavigator();
				return new StringNavigator(_value, _index + 1);
			}
		}

		/// <summary>
		/// This will try to extract a string of length requiredNumberOfCharacters from the current position in the string navigator. If there are insufficient
		/// characters available, then a string containing all of the remaining characters will be returned. This will be an empty string if there is no more
		/// content to deliver. This will never return null.
		/// </summary>
		public string TryToGetCharacterString(int requiredNumberOfCharacters)
		{
			if (requiredNumberOfCharacters <= 0)
				throw new ArgumentOutOfRangeException("requiredNumberOfCharacters", "must be greater than zero");

			if ((_index + requiredNumberOfCharacters) > _value.Length)
			{
				requiredNumberOfCharacters = _value.Length - _index;
				if (requiredNumberOfCharacters == 0)
					return "";
			}

			var result = new char[requiredNumberOfCharacters];
			Array.Copy(_value, _index, result, 0, requiredNumberOfCharacters);
			return new string(result);
		}
	}
}
