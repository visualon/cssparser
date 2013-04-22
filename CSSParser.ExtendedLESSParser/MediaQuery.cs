using System;
using System.Collections.Generic;
using System.Linq;

namespace CSSParser.ExtendedLESSParser
{
	/// <summary>
	/// This ContainerFragment represent a Media Query, its Selectors SelectorSet should only have a single value (unlike the Selector class which may have multiple
	/// Selectors - eg. "div#Header h2, div#Footer"). LESS CSS supports Media Queries that wrap styles and/or other selectors whilst regular CSS only supports the
	/// wrapping of selectors in Media Queries.
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
