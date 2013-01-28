using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CSSParser.ContentProcessors;
using CSSParser.ContentProcessors.StringProcessors;

namespace CSSRenderers
{
	public class PrettyPrintStyleHtmlRenderer : IRenderProcessedContent
	{
		/// <summary>
		/// This will throw an exception for a null segments reference or a segments set than contains any nulls. It will never return null.
		/// </summary>
		public string Render(IEnumerable<CategorisedCharacterString> segments)
		{
			if (segments == null)
				throw new ArgumentNullException("segments");

			var segmentsArray = segments.ToArray();
			if (segmentsArray.Any(s => s == null))
				throw new ArgumentException("Null reference encountered in segments set");

			var contentBuilder = new StringBuilder();
			for (var index = 0; index < segmentsArray.Length; index++)
			{
				var segment = segmentsArray[index];

				var optionalWrapperClass = GetWrapperClassIfAny(segmentsArray, index);
				if (optionalWrapperClass != null)
					contentBuilder.AppendFormat("<span class=\"{0}\">", HttpUtility.HtmlAttributeEncode(optionalWrapperClass));

				if (segment.CharacterCategorisation == CharacterCategorisationOptions.Whitespace)
				{
					// To be consistent with PrettyPrint formatting, we replace runs of spaces with "&nbsp;" and a single space at the end of the
					// run. Line breaks are replaced with the html element (any "\r\n" vs "\r" vs "\n" discrepancies are standardised first).
					var whiteSpaceContent = HttpUtility.HtmlEncode(segment.Value);
					while (whiteSpaceContent.Contains("  "))
						whiteSpaceContent = whiteSpaceContent.Replace("  ", "&nbsp; ");
					whiteSpaceContent = whiteSpaceContent.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "<br/>");
					contentBuilder.Append(whiteSpaceContent);
				}
				else
					contentBuilder.Append(HttpUtility.HtmlEncode(segment.Value));

				if (optionalWrapperClass != null)
					contentBuilder.Append("</span>");
			}
			return contentBuilder.ToString();
		}

		/// <summary>
		/// This will return null if the content should not be wrapped in an element. It will never return an empty string, it will be a non-empty value or null.
		/// </summary>
		private string GetWrapperClassIfAny(CategorisedCharacterString[] segments, int index)
		{
			if (segments == null)
				throw new ArgumentNullException("segments");
			if ((index < 0) || (index >= segments.Length))
				throw new ArgumentOutOfRangeException("index");

			var segment = segments[index];
			if (segment == null)
				throw new ArgumentException("Null reference encountered in segments array");

			if (segment.CharacterCategorisation == CharacterCategorisationOptions.Value)
				return "str";

			if ((segment.CharacterCategorisation == CharacterCategorisationOptions.CloseBrace)
			|| (segment.CharacterCategorisation == CharacterCategorisationOptions.OpenBrace)
			|| (segment.CharacterCategorisation == CharacterCategorisationOptions.SemiColon)
			|| (segment.CharacterCategorisation == CharacterCategorisationOptions.StylePropertyColon))
				return "pun";

			if (segment.CharacterCategorisation == CharacterCategorisationOptions.Comment)
				return "com";

			if (segment.CharacterCategorisation == CharacterCategorisationOptions.SelectorOrStyleProperty)
			{
				// The processors don't make a distinction between Selectors (eg. "div.Main > h2") and Styles (eg. "color") so in order to apply different
				// wrapper classes to them we have to look further along the segments data and try to find out if the next non-whitespace-or-comment
				// segment is a Colon - if so then it should be a Style Property.
				for (var nextIndex = index + 1; nextIndex < segments.Length; nextIndex++)
				{
					var furtherSegment = segments[nextIndex];
					if (furtherSegment == null)
						throw new ArgumentException("Null reference encountered in segments array");
					
					if (furtherSegment.CharacterCategorisation == CharacterCategorisationOptions.StylePropertyColon)
						return "kwd";
					if ((furtherSegment.CharacterCategorisation != CharacterCategorisationOptions.Whitespace)
					&& (furtherSegment.CharacterCategorisation != CharacterCategorisationOptions.Comment))
						break;
				}
				return "typ";
			}

			if (segment.CharacterCategorisation == CharacterCategorisationOptions.Whitespace)
				return "pln";

			throw new NotSupportedException("Unsupported CharacterCategorisationOptions: " + segment.CharacterCategorisation);
		}
	}
}
