using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSSParser.ContentProcessors;
using CSSParser.ContentProcessors.StringProcessors;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace CSSParser.ExtendedLESSParser
{
	/// <summary>
	/// This will take the data returned from the CSSParser.Parser methods and process it further into a hierarchical set of ICSSFragment instances
	/// where each fragment is a selector (which may represent a css selector - eg. "div.Header" - or media query) or style property names and
	/// values within the selectors. Selectors may be nested in the case of selectors within media queries or selectors nested within other
	/// selectors (as enabled by LESS CSS).
	/// </summary>
	public static class LessCssHierarchicalParser
	{
		public static IEnumerable<ICSSFragment> ParseIntoStructuredData(IEnumerable<CategorisedCharacterString> segments)
		{
			if (segments == null)
				throw new ArgumentNullException("segments");

			var segmentEnumerator = segments.GetEnumerator();
			var parsedData = ParseIntoStructuredData(segmentEnumerator, new Selector.SelectorSet[0], 0);
			while (segmentEnumerator.MoveNext())
			{
				var segment = segmentEnumerator.Current;
				if ((segment.CharacterCategorisation != CharacterCategorisationOptions.Comment)
				&& (segment.CharacterCategorisation != CharacterCategorisationOptions.Whitespace))
				{
					var lastFragment = parsedData.LastOrDefault();
					var lastFragmentLineIndex = (lastFragment == null) ? 0 : lastFragment.SourceLineIndex;
					throw new ArgumentException("Encountered unparsable data, this indicates content (after line " + (lastFragmentLineIndex + 1) + ")");
				}
			}
			return parsedData;
		}

		private static IEnumerable<ICSSFragment> ParseIntoStructuredData(
			IEnumerator<CategorisedCharacterString> segmentEnumerator,
			IEnumerable<Selector.SelectorSet> parentSelectors,
			int sourceLineIndex)
		{
			if (segmentEnumerator == null)
				throw new ArgumentNullException("segmentEnumerator");
			if (parentSelectors == null)
				throw new ArgumentNullException("parentSelectors");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");

			var fragments = new List<ICSSFragment>();
			var selectorOrStyleContentBuffer = new StringBuilder();
			var selectorOrStyleStartSourceLineIndex = -1;
			StylePropertyName lastStylePropertyName = null;
			var stylePropertyValueBuffer = new PropertyValueBuffer();
			while (segmentEnumerator.MoveNext())
			{
				var segment = segmentEnumerator.Current;
				if (segment == null)
					throw new ArgumentException("Null reference encountered in segments set");

				switch (segment.CharacterCategorisation)
				{
					case CharacterCategorisationOptions.Comment:
						sourceLineIndex += GetNumberOfLineReturnsFromContentIfAny(segment.Value);
						continue;

					case CharacterCategorisationOptions.Whitespace:
						sourceLineIndex += GetNumberOfLineReturnsFromContentIfAny(segment.Value);
						if (selectorOrStyleContentBuffer.Length > 0)
							selectorOrStyleContentBuffer.Append(" ");
						continue;

					case CharacterCategorisationOptions.SelectorOrStyleProperty:
						if (selectorOrStyleContentBuffer.Length == 0)
							selectorOrStyleStartSourceLineIndex = sourceLineIndex;
						selectorOrStyleContentBuffer.Append(segment.Value);

						// If we were building up content for a StylePropertyValue then encountering other content means that the value must have terminated
						// (for valid CSS it should be only a semicolon or close brace that terminates a value but we're not concerned about invalid CSS here)
						if (stylePropertyValueBuffer.HasContent)
							fragments.Add(stylePropertyValueBuffer.ExtractCombinedContentAndClear());
						continue;

					case CharacterCategorisationOptions.OpenBrace:
						if (selectorOrStyleContentBuffer.Length == 0)
							throw new ArgumentException("Encountered OpenBrace with no preceding selector at line " + (sourceLineIndex + 1));

						// If we were building up content for a StylePropertyValue then encountering other content means that the value must have terminated
						// (for valid CSS it should be only a semicolon or close brace that terminates a value but we're not concerned about invalid CSS here)
						if (stylePropertyValueBuffer.HasContent)
							fragments.Add(stylePropertyValueBuffer.ExtractCombinedContentAndClear());

						var selectors = GetSelectorSet(selectorOrStyleContentBuffer.ToString());
						if (selectors.First().Value.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase))
						{
							fragments.Add(new MediaQuery(
								selectors,
								parentSelectors,
								selectorOrStyleStartSourceLineIndex,
								ParseIntoStructuredData(segmentEnumerator, parentSelectors.Concat(new[] { selectors }), sourceLineIndex)
							));
						}
						else
						{
							fragments.Add(new Selector(
								selectors,
								parentSelectors,
								selectorOrStyleStartSourceLineIndex,
								ParseIntoStructuredData(segmentEnumerator, parentSelectors.Concat(new[] { selectors }), sourceLineIndex)
							));
						}
						selectorOrStyleContentBuffer.Clear();
						continue;

					case CharacterCategorisationOptions.CloseBrace:
						// If we were building up content for a StylePropertyValue then encountering other content means that the value must have terminated
						// (for valid CSS it should be only a semicolon or close brace that terminates a value but we're not concerned about invalid CSS here)
						if (stylePropertyValueBuffer.HasContent)
							fragments.Add(stylePropertyValueBuffer.ExtractCombinedContentAndClear());
						
						if (selectorOrStyleContentBuffer.Length > 0)
						{
							fragments.Add(new StylePropertyName(
								selectorOrStyleContentBuffer.ToString(),
								selectorOrStyleStartSourceLineIndex
							));
						}
						return fragments;

					case CharacterCategorisationOptions.StylePropertyColon:
					case CharacterCategorisationOptions.SemiColon:
						// If we were building up content for a StylePropertyValue then encountering other content means that the value must have terminated
						// (for valid CSS it should be only a semicolon or close brace that terminates a value but we're not concerned about invalid CSS here)
						if (stylePropertyValueBuffer.HasContent)
							fragments.Add(stylePropertyValueBuffer.ExtractCombinedContentAndClear());

						if (selectorOrStyleContentBuffer.Length > 0)
						{
							var selectorOrStyleContent = selectorOrStyleContentBuffer.ToString();
							if (selectorOrStyleContent.StartsWith("@import", StringComparison.InvariantCultureIgnoreCase))
							{
								fragments.Add(new Import(
									selectorOrStyleContent.Substring("@import".Length).Trim(),
									sourceLineIndex
								));
								selectorOrStyleContentBuffer.Clear();
								continue;
							}

							// Note: The SemiColon case here probably suggests invalid content, it should only follow a Value segment (ignoring
							// Comments and WhiteSpace), so if there is anything in the selectorOrStyleContentBuffer before the SemiColon then
							// it's probably not correct (but we're not validating for that here, we just don't want to throw anything away!)
							lastStylePropertyName = new StylePropertyName(
								selectorOrStyleContentBuffer.ToString(),
								selectorOrStyleStartSourceLineIndex
							);
							fragments.Add(lastStylePropertyName);
							selectorOrStyleContentBuffer.Clear();
						}
						continue;

					case CharacterCategorisationOptions.Value:
						if (selectorOrStyleContentBuffer.Length > 0)
						{
							var selectorOrStyleContent = selectorOrStyleContentBuffer.ToString();
							if (selectorOrStyleContent.StartsWith("@import", StringComparison.InvariantCultureIgnoreCase))
							{
								selectorOrStyleContentBuffer.Append(segment.Value);
								continue;
							}

							// This is presumably an error condition, there should be a colon between SelectorOrStyleProperty content and
							// Value content, but we're not validating here so just lump it all together
							lastStylePropertyName = new StylePropertyName(
								selectorOrStyleContentBuffer.ToString(),
								selectorOrStyleStartSourceLineIndex
							);
							fragments.Add(lastStylePropertyName);
							selectorOrStyleContentBuffer.Clear();
						}
						if (lastStylePropertyName == null)
							throw new Exception("Invalid content, orphan style property value encountered");
						stylePropertyValueBuffer.Add(new StylePropertyValue(
							lastStylePropertyName,
							new[] { segment.Value },
							selectorOrStyleStartSourceLineIndex
						));
						continue;

					default:
						throw new ArgumentException("Unsupported CharacterCategorisationOptions value: " + segment.CharacterCategorisation);
				}
			}

			// If we have any content in the selectorOrStyleContentBuffer and we're hitting a CloseBrace then it's probably invalid content,
			// but just stash it away and move on! (The purpose of this work isn't to get too nuts about invalid CSS).
			if (selectorOrStyleContentBuffer.Length > 0)
			{
				var selectors = GetSelectorSet(selectorOrStyleContentBuffer.ToString());
				if (selectors.First().Value.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase))
				{
					fragments.Add(new MediaQuery(
						selectors,
						parentSelectors,
						sourceLineIndex,
						new ICSSFragment[0]
					));
				}
				else
				{
					fragments.Add(new Selector(
						selectors,
						parentSelectors,
						sourceLineIndex,
						new ICSSFragment[0]
					));
				}
			}

			// It's very feasible that there will still be some style property value content in the buffer at this point, so ensure it
			// doesn't get lost
			if (stylePropertyValueBuffer.HasContent)
				fragments.Add(stylePropertyValueBuffer.ExtractCombinedContentAndClear());

			return fragments;
		}

		private static Selector.SelectorSet GetSelectorSet(string selectors)
		{
			if (string.IsNullOrWhiteSpace(selectors))
				throw new ArgumentException("Null/blank selectors specified");

			return new Selector.SelectorSet(
				selectors
					.Split(',')
					.Select(s => s.Trim())
					.Where(s => s != "")
					.Select(s => new Selector.WhiteSpaceNormalisedString(s))
			);
		}

		private class PropertyValueBuffer
		{
			private readonly List<StylePropertyValue> _values = new List<StylePropertyValue>();
			public void Add(StylePropertyValue value)
			{
				if (value == null)
					throw new ArgumentNullException("value");
				if (_values.Any() && (_values.First().Property.Value != value.Property.Value))
					throw new ArgumentException("All values must relate to the same property");
				_values.Add(value);
			}
			public bool HasContent
			{
				get { return _values.Any(); }
			}
			public StylePropertyValue ExtractCombinedContentAndClear()
			{
				if (!_values.Any())
					throw new Exception("No content to retrieve");
				var combinedContent = new StylePropertyValue(
					_values.First().Property,
					_values.SelectMany(v => v.ValueSegments),
					_values.First().SourceLineIndex
				);
				_values.Clear();
				return combinedContent;
			}
		}

		private static int GetNumberOfLineReturnsFromContentIfAny(string content)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			return content.Replace("\r\n", "\n").Replace("\r", "\n").Count(c => c == '\n');
		}
	}
}
