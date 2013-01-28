using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class SelectorOrStylePropertySegment : SelectorOrStyleSegment
	{
		public SelectorOrStylePropertySegment(SingleLineCommentsSupportOptions singleLineCommentsSupportOptions, IGenerateCharacterProcessors processorFactory)
			: base(ProcessingTypeOptions.StyleOrSelector, singleLineCommentsSupportOptions, processorFactory) { }
	}
}
