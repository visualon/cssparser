using System;
using System.Collections.Generic;
using System.Linq;

namespace CSSParser.ExtendedLESSParser
{
	/// <summary>
	/// This ContainerFragment represent a Media Query that whose ChildFragments should all be Selectors (if the source content was valid), its Selectors SelectorSet
	/// should only have a single value (unlike the Selector class which may have multiple Selectors - eg. "div#Header h2, div#Footer")
	/// </summary>
	public class MediaQuery : ContainerFragment
	{
		public MediaQuery(
			SelectorSet selectors,
			IEnumerable<SelectorSet> parentSelectors,
			int sourceLineIndex,
			IEnumerable<ICSSFragment> childFragments) : base(selectors, parentSelectors, sourceLineIndex, childFragments)
		{
			// The "selectors" argument can't be null at this point as the ContainerFragment base class would have already thrown an ArgumentNullException
			if (!selectors.First().Value.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase))
				throw new ArgumentException("The content indicates that this should be a Selector, not a Media Query");
		}
	}
}
