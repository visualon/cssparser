using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public abstract class SelectorOrStyleSegment : IProcessCharacters
	{
		private readonly ProcessingTypeOptions _processingType;
		private readonly SingleLineCommentsSupportOptions _singleLineCommentsSupportOptions;
		private readonly CharacterCategorisationBehaviourOverride _optionalCharacterCategorisationBehaviourOverride;
		private readonly IGenerateCharacterProcessors _processorFactory;
		protected SelectorOrStyleSegment(
			ProcessingTypeOptions processingType,
			SingleLineCommentsSupportOptions singleLineCommentsSupportOptions,
			CharacterCategorisationBehaviourOverride optionalCharacterCategorisationBehaviourOverride,
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
			_optionalCharacterCategorisationBehaviourOverride = optionalCharacterCategorisationBehaviourOverride;
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

		public class CharacterCategorisationBehaviourOverride
		{
			public CharacterCategorisationBehaviourOverride(
				char endOfBehaviourOverrideCharacter,
				CharacterCategorisationOptions characterCategorisation,
				IProcessCharacters characterProcessorToReturnTo)
			{
				if (!Enum.IsDefined(typeof(CharacterCategorisationOptions), characterCategorisation))
					throw new ArgumentOutOfRangeException("characterCategorisation");
				if (characterProcessorToReturnTo == null)
					throw new ArgumentNullException("characterProcessorToReturnTo");

				EndOfBehaviourOverrideCharacter = endOfBehaviourOverrideCharacter;
				CharacterCategorisation = characterCategorisation;
				CharacterProcessorToReturnTo = characterProcessorToReturnTo;
			}

			public char EndOfBehaviourOverrideCharacter { get; private set; }
	
			public CharacterCategorisationOptions CharacterCategorisation { get; private set; }

			/// <summary>
			/// This will never be null
			/// </summary>
			public IProcessCharacters CharacterProcessorToReturnTo { get; private set; }
		}

		public CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			// If dealing with encountering a single special character such as braces, colons or whitespace then record the character type and
			// continue processing. Some characters may require the processingStyleOrSelector flag to be reset; eg. if we're in "selector" content
			// and process a colon, then this indicates that we are entering "value" content (likewise, if we're in "value" content and encounter
			// a brace then we must have left the value).
			// - If there is an optionalCharacterCategorisationBehaviourOverride value then most of these single special characters are ignored
			//   and are forced into being categorised as something else (this is used by the code the ensures that attribute selectors and
			//   LESS mixin argument sets are identified as being SelectorOrStyleProperty content even it could contain whitespace, quoted
			//   sections and all sort). The optionalCharacterCategorisationBehaviourOverride does not apply to comment content, that is
			//   always identified as being type Comment even if it's inside an attribute selector.

			// Is this the end of the section that the optionalCharacterCategorisationBehaviourOverride (if non-null) is concerned with? If so
			// then drop back out to the character processor that handed control over to the optionalCharacterCategorisationBehaviourOverride.
			var currentCharacter = stringNavigator.CurrentCharacter;
			if ((_optionalCharacterCategorisationBehaviourOverride != null)
			&& (currentCharacter == _optionalCharacterCategorisationBehaviourOverride.EndOfBehaviourOverrideCharacter))
			{
				return new CharacterProcessorResult(
					_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
					_optionalCharacterCategorisationBehaviourOverride.CharacterProcessorToReturnTo
				);
			}

			// Deal with other special characters (bearing in mind the altered interactions if optionalCharacterCategorisationBehaviourOverride
			// is non-null)
			if (currentCharacter == '{')
			{
				if (_optionalCharacterCategorisationBehaviourOverride != null)
				{
					return new CharacterProcessorResult(
						_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
						this
					);
				}
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.OpenBrace,
					GetSelectorOrStyleCharacterProcessor()
				);
			}
			else if (currentCharacter == '}')
			{
				if (_optionalCharacterCategorisationBehaviourOverride != null)
				{
					return new CharacterProcessorResult(
						_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
						this
					);
				}
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.CloseBrace,
					GetSelectorOrStyleCharacterProcessor()
				);
			}
			else if (currentCharacter == ';')
			{
				if (_optionalCharacterCategorisationBehaviourOverride != null)
				{
					return new CharacterProcessorResult(
						_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
						this
					);
				}
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.SemiColon,
					GetSelectorOrStyleCharacterProcessor()
				);
			}
			else if (currentCharacter == ':')
			{
				if (_optionalCharacterCategorisationBehaviourOverride != null)
				{
					return new CharacterProcessorResult(
						_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
						this
					);
				}
				
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
			else if ((currentCharacter != null) && char.IsWhiteSpace(currentCharacter.Value))
			{
				if (_optionalCharacterCategorisationBehaviourOverride != null)
				{
					return new CharacterProcessorResult(
						_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
						this
					);
				}
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Whitespace,
					this
				);
			}

			// To deal with comments we use specialised comment-handling processors (even if an optionalCharacterCategorisationBehaviourOverride
			// is specified we still treat deal with comments as normal, their content is not forced into a different categorisation)
			var nextCharacter = stringNavigator.Next.CurrentCharacter;
			if ((_singleLineCommentsSupportOptions == SingleLineCommentsSupportOptions.Support) && (currentCharacter == '/') && (nextCharacter == '/'))
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Comment,
					_processorFactory.Get<SingleLineCommentSegment>(this, _processorFactory)
				);
			}
			if ((currentCharacter == '/') && (nextCharacter == '*'))
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Comment,
					_processorFactory.Get<MultiLineCommentSegment>(this, _processorFactory)
				);
			}

			// Although media query declarations will be marked as SelectorOrStyleProperty content, special handling is required to ensure that
			// any colons that exist in it are identified as part of the SelectorOrStyleProperty and not marked as a StylePropertyColon
			if ((_processingType == ProcessingTypeOptions.StyleOrSelector) && stringNavigator.DoesCurrentContentMatch("@media"))
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.SelectorOrStyleProperty,
					_processorFactory.Get<MediaQuerySegment>(this)
				);
			}

			// If we encounter quotes then we need to use the QuotedSegment which will keep track of where the quoted section end (taking
			// into account any escape sequences)
			if ((currentCharacter == '"') || (currentCharacter == '\''))
			{
				// If an optionalCharacterCategorisationBehaviourOverride was specified then the content will be identified as whatever
				// categorisation is specified by it, otherwise it will be identified as being CharacterCategorisationOptions.Value
				if (_optionalCharacterCategorisationBehaviourOverride != null)
				{
					return new CharacterProcessorResult(
						_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
						_processorFactory.Get<QuotedSegment>(
							currentCharacter,
							_optionalCharacterCategorisationBehaviourOverride.CharacterCategorisation,
							this,
							_processorFactory
						)
					);
				}
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Value,
					_processorFactory.Get<QuotedSegment>(
						currentCharacter,
						CharacterCategorisationOptions.Value,
						GetValueCharacterProcessor(),
						_processorFactory
					)
				);
			}

			// If we're currently processing StyleOrSelector content and we encounter a square or round open bracket then we're about to
			// enter an attribute selector (eg. "a[href]") or a LESS mixin argument set (eg. ".RoundedCorners (@radius)". In either case
			// we need to consider all content until the corresponding close bracket to be a StyleOrSelector, whether it's whitespace or
			// a quoted section (note: not if it's a comment, that still gets identified as comment content).
			if (_processingType == ProcessingTypeOptions.StyleOrSelector)
			{
				char? closingBracket;
				if (currentCharacter == '[')
					closingBracket = ']';
				else if (currentCharacter == '(')
					closingBracket = ')';
				else
					closingBracket = null;
				if (closingBracket != null)
				{
					return new CharacterProcessorResult(
						CharacterCategorisationOptions.SelectorOrStyleProperty,
						_processorFactory.Get<BracketedSelectorSegment>(
							_singleLineCommentsSupportOptions,
							closingBracket.Value,
							this,
							_processorFactory
						)
					);
				}
			}

			// If it's not a quoted or bracketed section, then we can continue to use this instance to process the content
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
			while (true)
			{
				var character = stringNavigator.CurrentCharacter;
				if ((character == null) || !char.IsWhiteSpace(character.Value))
					break;
				stringNavigator = stringNavigator.Next;
			}

			// Determine whether that content (if there is any) matches any of the pseudo classes
			return PseudoClasses.Any(c => stringNavigator.DoesCurrentContentMatch(c));
		}

		/// <summary>
		/// These are ordered by length as a very minor optimisation, it may allow matches to occur more quickly since less characters may have to be tested
		/// (to be honest, it would probably make more sense to arrange them in order of likelihood that they will appear and the most expensive case is when
		/// none of them are present and no ordering will help with that)
		/// </summary>
		private static readonly ReadOnlyCollection<string> PseudoClasses = new List<string>
		{
			"lang",
            "link",
            "after",
            "focus",
            "hover",
            "active",
            "before",
            "visited",
            "first-line",
            "first-child",
            "first-letter",
            "last-child",
            "nth-child",
            "checked",
            "disabled",
            "empty",
            "enabled",
            "first-of-type",
            "focus",
            "in-range",
            "invalid",
            "last-of-type",
            "not",
            "nth-last-child",
            "nth-last-of-type",
            "nth-of-type",
            "only-of-type",
            "only-child",
            "optional",
            "out-of-range",
            "read-only",
            "read-write",
            "required",
            "root",
            "target",
            "valid"
		}.AsReadOnly();
	}
}
