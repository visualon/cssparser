using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	/// <summary>
	/// This will likely be an attribute selector (eg. the "[type='text']" of "input[type='text']") or the argument list of a LESS mixin. The
	/// entire content will be identified as CharacterCategorisationOptions.SelectorOrStyleProperty, such as whitespace, quoted sections (but
	/// not comments - they will still be identified as acterCategorisationOptions.Comment).
	/// </summary>
	public class BracketedSelectorSegment : SelectorOrStyleSegment
	{
		public BracketedSelectorSegment(
			SingleLineCommentsSupportOptions singleLineCommentsSupportOptions,
			char closeBracketCharacter,
			IProcessCharacters characterProcessorToReturnTo,
			IGenerateCharacterProcessors processorFactory)
				: base(
					ProcessingTypeOptions.StyleOrSelector,
					singleLineCommentsSupportOptions,
					new CharacterCategorisationBehaviourOverride(
						closeBracketCharacter,
						CharacterCategorisationOptions.SelectorOrStyleProperty,
						characterProcessorToReturnTo
					),
					processorFactory
				) { }
	}
}
