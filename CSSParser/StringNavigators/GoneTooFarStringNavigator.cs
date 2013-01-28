using System;

namespace CSSParser.StringNavigators
{
	public abstract class GoneTooFarStringNavigator : IWalkThroughStrings
	{
		protected readonly string _value;
		protected readonly int _distanceTooFar;
		public GoneTooFarStringNavigator(string value, int distanceTooFar)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			if (distanceTooFar <= 0)
				throw new ArgumentOutOfRangeException("distanceTooFar", "must be greater than zero");

			_value = value;
			_distanceTooFar = distanceTooFar;
		}

		public char? CurrentCharacter { get { return null; } }

		public abstract IWalkThroughStrings Previous { get; }

		public abstract IWalkThroughStrings Next { get; }
	}
}
