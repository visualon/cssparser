namespace CSSParser.StringNavigators
{
	public class GoneTooFarForwardStringNavigator : GoneTooFarStringNavigator
	{
		public GoneTooFarForwardStringNavigator(string value, int distanceTooFar) : base(value, distanceTooFar) { }

		public override IWalkThroughStrings Previous
		{
			get
			{
				if (_distanceTooFar == 1)
				{
					if (_value.Length == 0)
						return new GoneTooFarBackwardStringNavigator(_value, 1);
					return new StringNavigator(_value);
				}
				return new GoneTooFarForwardStringNavigator(_value, _distanceTooFar - 1);
			}
		}

		public override IWalkThroughStrings Next
		{
			get { return new GoneTooFarForwardStringNavigator(_value, _distanceTooFar + 1); }
		}
	}
}
