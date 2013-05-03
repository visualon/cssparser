using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class StyleValueSegment : SelectorOrStyleSegment
	{
		public StyleValueSegment(SingleLineCommentsSupportOptions singleLineCommentsSupportOptions, IGenerateCharacterProcessors processorFactory)
			: base(ProcessingTypeOptions.Value, singleLineCommentsSupportOptions, null, processorFactory) { }
	}
}
