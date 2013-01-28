using System.Collections.Generic;
using CSSParser.ContentProcessors.StringProcessors;

namespace CSSRenderers
{
	public interface IRenderProcessedContent
	{
		/// <summary>
		/// This will throw an exception for a null segments reference or a segments set than contains any nulls. It will never return null.
		/// </summary>
		string Render(IEnumerable<CategorisedCharacterString> segments);
	}
}
