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
		char? IWalkThroughStrings.CurrentCharacter { get { return CurrentCharacter; } }

		public IWalkThroughStrings Previous
		{
			get
			{
				if (_index == 0)
					return new GoneTooFarBackwardStringNavigator(_value, 1);
				return new StringNavigator(_value, _index - 1);
			}
		}

		public IWalkThroughStrings Next
		{
			get
			{
				if (_index == _value.Length - 1)
					return new GoneTooFarForwardStringNavigator(_value, 1);
				return new StringNavigator(_value, _index + 1);
			}
		}
	}
}
