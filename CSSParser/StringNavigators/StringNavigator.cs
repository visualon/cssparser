using System;

namespace CSSParser.StringNavigators
{
	public class StringNavigator : IWalkThroughStrings
	{
		private readonly string _value;
		private readonly int _index;
		private StringNavigator(string value, int index)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException("Null/empty value specified");
			if ((index < 0) || (index >= value.Length))
				throw new ArgumentOutOfRangeException("index");

			_value = value;
			_index = index;
		}
		public StringNavigator(string value) : this(value, 0) { }

		public char CurrentCharacter
		{
			get { return _value[_index]; }
		}

		/// <summary>
		/// This return null if the current location in the string has no content (eg. anywhere on an empty string or past the end of a non-empty string)
		/// </summary>
		char? IWalkThroughStrings.CurrentCharacter { get { return CurrentCharacter; } }

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
				requiredNumberOfCharacters = _value.Length - _index;

			return _value.Substring(_index, requiredNumberOfCharacters);
		}
	}
}
