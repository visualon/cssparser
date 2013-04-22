using System;
using System.Collections.Generic;
using System.Linq;

namespace CSSParser.ExtendedLESSParser
{
	/// <summary>
	/// This is a CSS fragment that represents a selector string, it may be multiple comma-separated selectors (if so, then the Selectors property will have multiple entries)
	/// </summary>
	public class Selector : ContainerFragment
	{
		public Selector(
			SelectorSet selectors,
			IEnumerable<SelectorSet> parentSelectors,
			int sourceLineIndex,
			IEnumerable<ICSSFragment> childFragments) : base(selectors, parentSelectors, sourceLineIndex, childFragments)
		{
			// The "selectors" argument can't be null at this point as the ContainerFragment base class would have already thrown an ArgumentNullException
			if (selectors.First().Value.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase))
				throw new ArgumentException("The content indicates that this should be a Media Query, not a Selector");
		}
	}
}
