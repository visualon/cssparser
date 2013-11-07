using System;
using System.Collections.Generic;
using System.Text;

namespace CSSParser.ExtendedLESSParser.Misc
{
	public static class SelectorBreaker
	{
		/// <summary>
		/// Given a selectors string (eg. "div.formData input[type='hidden'], div.entry p label"), return a set of selectors where each selector is broken
		/// into selector segment strings (eg. [ [ "div.formData", "input[type='hidden']" ], [ "div.entry", "p", "label" ] ])
		/// </summary>
		public static IEnumerable<IEnumerable<string>> Break(string selectorsString)
		{
			if (selectorsString == null)
				throw new ArgumentNullException("selectors");

			var selectors = new List<IEnumerable<string>>();
			var selectorBuffer = new List<string>();
			var selectorSegmentBuffer = new StringBuilder();
			IProcessSelectorContent processor = new DefaultSelectorProcessor();
			for (var index = 0; index <= selectorsString.Length; index++)
			{
				char c;
				SelectorProcessorResult result;
				if (index == selectorsString.Length)
				{
					c = '\0';
					result = new SelectorProcessorResult(CharacterCategorisationOptions.EndOfSelector, processor);
				}
				else
				{
					c = selectorsString[index];
					result = processor.Process(c);
				}
				if (result.CharacterCategorisation == CharacterCategorisationOptions.SelectorSegment)
					selectorSegmentBuffer.Append(c);
				else if (result.CharacterCategorisation == CharacterCategorisationOptions.EndOfSelectorSegment)
				{
					if (selectorSegmentBuffer.Length > 0)
					{
						selectorBuffer.Add(selectorSegmentBuffer.ToString());
						selectorSegmentBuffer.Clear();
					}
				}
				else if (result.CharacterCategorisation == CharacterCategorisationOptions.EndOfSelector)
				{
					if (selectorSegmentBuffer.Length > 0)
					{
						selectorBuffer.Add(selectorSegmentBuffer.ToString());
						selectorSegmentBuffer.Clear();
					}
					if (selectorBuffer.Count > 0)
					{
						selectors.Add(selectorBuffer.ToArray());
						selectorBuffer.Clear();
					}
				}
				else
					throw new NotSupportedException("Unsupported CharacterCategorisationOptions: " + result.CharacterCategorisation);

				processor = result.NextProcessor;
			}
			return selectors;
		}

		private class DefaultSelectorProcessor : IProcessSelectorContent
		{
			/// <summary>
			/// This will never return null, it will raise an exception if unable to process the content
			/// </summary>
			public SelectorProcessorResult Process(char currentCharacter)
			{
				if (Char.IsWhiteSpace(currentCharacter))
					return new SelectorProcessorResult(CharacterCategorisationOptions.EndOfSelectorSegment, this);
				else if (currentCharacter == ',')
					return new SelectorProcessorResult(CharacterCategorisationOptions.EndOfSelector, this);
				else if (currentCharacter == '[')
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, new AttributeSelectorProcessor(this));
				else
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, this);
			}
		}

		private class AttributeSelectorProcessor : IProcessSelectorContent
		{
			private readonly IProcessSelectorContent _previousProcessor;
			public AttributeSelectorProcessor(IProcessSelectorContent previousProcessor)
			{
				if (previousProcessor == null)
					throw new ArgumentNullException("previousProcessor");

				_previousProcessor = previousProcessor;
			}

			/// <summary>
			/// This will never return null, it will raise an exception if unable to process the content
			/// </summary>
			public SelectorProcessorResult Process(char currentCharacter)
			{
				if ((currentCharacter == '\"') || (currentCharacter == '\''))
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, new QuotedSelectorProcessor(this, currentCharacter));
				else if (currentCharacter == ']')
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, _previousProcessor);
				else
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, this);
			}
		}

		private class QuotedSelectorProcessor : IProcessSelectorContent
		{
			private readonly IProcessSelectorContent _previousProcessor;
			private readonly char _quoteCharacter;
			private readonly bool _nextCharacterIsEscaped;
			public QuotedSelectorProcessor(IProcessSelectorContent previousProcessor, char quoteCharacter) : this(previousProcessor, quoteCharacter, false) { }
			private QuotedSelectorProcessor(IProcessSelectorContent previousProcessor, char quoteCharacter, bool nextCharacterIsEscaped)
			{
				if (previousProcessor == null)
					throw new ArgumentNullException("previousProcessor");

				_previousProcessor = previousProcessor;
				_quoteCharacter = quoteCharacter;
				_nextCharacterIsEscaped = nextCharacterIsEscaped;
			}

			/// <summary>
			/// This will never return null, it will raise an exception if unable to process the content
			/// </summary>
			public SelectorProcessorResult Process(char currentCharacter)
			{
				if (_nextCharacterIsEscaped)
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, new QuotedSelectorProcessor(_previousProcessor, _quoteCharacter, false));
				else if (currentCharacter == _quoteCharacter)
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, _previousProcessor);
				else
					return new SelectorProcessorResult(CharacterCategorisationOptions.SelectorSegment, this);
			}
		}

		private interface IProcessSelectorContent
		{
			/// <summary>
			/// This will never return null, it will raise an exception if unable to process the content
			/// </summary>
			SelectorProcessorResult Process(char currentCharacter);
		}

		private class SelectorProcessorResult
		{
			public SelectorProcessorResult(CharacterCategorisationOptions characterCategorisation, IProcessSelectorContent nextProcessor)
			{
				if (!Enum.IsDefined(typeof(CharacterCategorisationOptions), characterCategorisation))
					throw new ArgumentOutOfRangeException("characterCategorisation");
				if (nextProcessor == null)
					throw new ArgumentNullException("nextProcessor");

				CharacterCategorisation = characterCategorisation;
				NextProcessor = nextProcessor;
			}

			public CharacterCategorisationOptions CharacterCategorisation { get; private set; }

			/// <summary>
			/// This will never be null
			/// </summary>
			public IProcessSelectorContent NextProcessor { get; private set; }
		}

		private enum CharacterCategorisationOptions
		{
			SelectorSegment,
			EndOfSelectorSegment,
			EndOfSelector
		}
	}
}
