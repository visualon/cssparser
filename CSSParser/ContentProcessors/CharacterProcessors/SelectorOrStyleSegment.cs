using System;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public abstract class SelectorOrStyleSegment : IProcessCharacters
	{
		private readonly ProcessingTypeOptions _processingType;
		private readonly IGenerateCharacterProcessors _processorFactory;
		protected SelectorOrStyleSegment(ProcessingTypeOptions processingType, IGenerateCharacterProcessors processorFactory)
		{
			if (!Enum.IsDefined(typeof(ProcessingTypeOptions), processingType))
				throw new ArgumentOutOfRangeException("processingType");
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");

			_processingType = processingType;
			_processorFactory = processorFactory;
		}

		protected enum ProcessingTypeOptions
		{
			StyleOrSelector,
			Value
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
			if ((stringNavigator.TryToGetCharacterString(2) ?? "") == "//")
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Comment,
					_processorFactory.Get<CssSingleLineCommentSegment>(this)
				);
			}
			if ((stringNavigator.TryToGetCharacterString(2) ?? "") == "/*")
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Comment,
					_processorFactory.Get<MultiLineCommentSegment>(this, _processorFactory)
				);
			}

			if ((stringNavigator.TryToGetCharacterString(6) ?? "").Equals("@media", StringComparison.InvariantCultureIgnoreCase))
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
						GetSelectorOrStyleCharacterProcessor(),
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
			return (_processingType == ProcessingTypeOptions.StyleOrSelector) ? this : _processorFactory.Get<SelectorOrStylePropertySegment>(_processorFactory);
		}

		private IProcessCharacters GetValueCharacterProcessor()
		{
			return (_processingType == ProcessingTypeOptions.Value) ? this : _processorFactory.Get<StyleValueSegment>(_processorFactory);
		}
	}
}
