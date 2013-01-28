namespace CSSParser.StringNavigators
{
	public class GoneTooFarBackwardStringNavigator : GoneTooFarStringNavigator
	{
		public GoneTooFarBackwardStringNavigator(string value, int distanceTooFar) : base(value, distanceTooFar) { }

		public override IWalkThroughStrings Previous
		{
			get { return new GoneTooFarForwardStringNavigator(_value, _distanceTooFar + 1); }
		}

		public override IWalkThroughStrings Next
		{
			get
			{
				if (_distanceTooFar == 1)
				{
					if (_value.Length == 0)
						return new GoneTooFarForwardStringNavigator(_value, 1);
					return new StringNavigator(_value);
				}
				return new GoneTooFarBackwardStringNavigator(_value, _distanceTooFar - 1);
			}
		}
	}
}
