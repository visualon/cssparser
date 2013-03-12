using System;
using System.Linq;
using System.Text;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public abstract class SelectorOrStyleSegment : IProcessCharacters
	{
		private readonly ProcessingTypeOptions _processingType;
		private readonly SingleLineCommentsSupportOptions _singleLineCommentsSupportOptions;
		private readonly IGenerateCharacterProcessors _processorFactory;
		protected SelectorOrStyleSegment(
			ProcessingTypeOptions processingType,
			SingleLineCommentsSupportOptions singleLineCommentsSupportOptions,
			IGenerateCharacterProcessors processorFactory)
		{
			if (!Enum.IsDefined(typeof(ProcessingTypeOptions), processingType))
				throw new ArgumentOutOfRangeException("processingType");
			if (!Enum.IsDefined(typeof(SingleLineCommentsSupportOptions), singleLineCommentsSupportOptions))
				throw new ArgumentOutOfRangeException("singleLineCommentsSupportOptions");
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");

			_processingType = processingType;
			_singleLineCommentsSupportOptions = singleLineCommentsSupportOptions;
			_processorFactory = processorFactory;
		}

		protected enum ProcessingTypeOptions
		{
			StyleOrSelector,
			Value
		}

		public enum SingleLineCommentsSupportOptions
		{
			DoNotSupport,
			Support
		}

		public CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			// If dealing encountering a single special character such as braces, colons or whitespace then record the character type and continue
			// processing. Some characters may require the processingStyleOrSelector flag to be reset; eg. if we're in "selector" content and
			// process a colon, then this indicates that we are entering "value" content (likewise, if we're in "value" content and encounter
			// a brace then we must have left the value).
			if (stringNavigator.CurrentCharacter == '{')
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.OpenBrace,
					GetSelectorOrStyleCharacterProcessor()
				);
			}
			else if (stringNavigator.CurrentCharacter == '}')
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.CloseBrace,
					GetSelectorOrStyleCharacterProcessor()
				);
			}
			else if (stringNavigator.CurrentCharacter == ';')
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.SemiColon,
					GetSelectorOrStyleCharacterProcessor()
				);
			}
			else if (stringNavigator.CurrentCharacter == ':')
			{
				// If the colon indicates a pseudo-class for a selector then we want to continue processing it as a selector and not presume
				// that the content type has switched to a value (this is more complicated with LESS nesting to support, if it was just CSS
				// then things would have been easier!)
				if (_processingType == ProcessingTypeOptions.StyleOrSelector)
				{
					if (IsNextWordOneOfThePseudoClasses(stringNavigator.Next))
					{
						return new CharacterProcessorResult(
							CharacterCategorisationOptions.SelectorOrStyleProperty,
							GetSelectorOrStyleCharacterProcessor()
						);
					}
				}
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.StylePropertyColon,
					GetValueCharacterProcessor()
				);
			}
			else if ((stringNavigator.CurrentCharacter != null) && char.IsWhiteSpace(stringNavigator.CurrentCharacter.Value))
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Whitespace,
					this
				);
			}

			// To deal with comments we use specialised comment-handling processors
			if ((_singleLineCommentsSupportOptions == SingleLineCommentsSupportOptions.Support) && (stringNavigator.TryToGetCharacterString(2) == "//"))
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Comment,
					_processorFactory.Get<SingleLineCommentSegment>(this, _processorFactory)
				);
			}
			if (stringNavigator.TryToGetCharacterString(2) == "/*")
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Comment,
					_processorFactory.Get<MultiLineCommentSegment>(this, _processorFactory)
				);
			}

			if (stringNavigator.TryToGetCharacterString(6).Equals("@media", StringComparison.InvariantCultureIgnoreCase))
			{
				// Although media query declarations will be marked as SelectorOrStyleProperty content, special handling is required to
				// ensure that any colons that exist in it are identified as part of the SelectorOrStyleProperty and not marked as a
				// StylePropertyColon
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.SelectorOrStyleProperty,
					_processorFactory.Get<MediaQuerySegment>(this)
				);
			}

			// If we encounter then we need to use the QuotedValueSegment which will keep track of where the quoted section end (taking
			// into account any escape sequences)
			if ((stringNavigator.CurrentCharacter == '"') || (stringNavigator.CurrentCharacter == '\''))
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Value,
					_processorFactory.Get<QuotedValueSegment>(
						stringNavigator.CurrentCharacter.Value,
						GetValueCharacterProcessor(),
						_processorFactory
					)
				);
			}

			// If it's not a quoted section, then we can use this class to process the Value content
			return new CharacterProcessorResult(
				_processingType == ProcessingTypeOptions.StyleOrSelector
					? CharacterCategorisationOptions.SelectorOrStyleProperty
					: CharacterCategorisationOptions.Value,
				this
			);
		}

		private IProcessCharacters GetSelectorOrStyleCharacterProcessor()
		{
			return (_processingType == ProcessingTypeOptions.StyleOrSelector)
				? this
				: _processorFactory.Get<SelectorOrStylePropertySegment>(_singleLineCommentsSupportOptions, _processorFactory);
		}

		private IProcessCharacters GetValueCharacterProcessor()
		{
			return (_processingType == ProcessingTypeOptions.Value)
				? this
				: _processorFactory.Get<StyleValueSegment>(_singleLineCommentsSupportOptions, _processorFactory);
		}

		/// <summary>
		/// This will try to determine whether the next word from the given point in the string navigator is a recognised pseudo class. Any whitespace at
		/// the current position will be moved over and the content, if any, taken from there.
		/// </summary>
		private bool IsNextWordOneOfThePseudoClasses(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			// Skip over any whitespace to find the start of the next content
			while ((stringNavigator.CurrentCharacter != null) && char.IsWhiteSpace(stringNavigator.CurrentCharacter.Value))
				stringNavigator = stringNavigator.Next;

			// Determine whether that content (if there is any) matches any of the pseudo classes
			foreach (var pseudoClassGroupedByLength in PseudoClasses.GroupBy(v => v.Length).Select(g => new { Length = g.Key, Values = g.ToArray() }).OrderBy(g => g.Length))
			{
				var content = stringNavigator.TryToGetCharacterString(pseudoClassGroupedByLength.Length);
				if (content.Length < pseudoClassGroupedByLength.Length)
				{
					// There is not enough content remaining in the string navigator for this group or any following group, since they are ordered by length
					return false;
				}
				if (pseudoClassGroupedByLength.Values.Contains(content))
					return true;
			}
			return false;
		}

		private static readonly string[] PseudoClasses = new[] { "link", "visited", "active", "hover", "focus", "first-letter", "first-line", "first-child", "before", "after", "lang" };
	}
}
