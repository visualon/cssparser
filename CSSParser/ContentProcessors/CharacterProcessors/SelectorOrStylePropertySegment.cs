using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class SelectorOrStylePropertySegment : SelectorOrStyleSegment
	{
		public SelectorOrStylePropertySegment(IGenerateCharacterProcessors processorFactory) : base(ProcessingTypeOptions.StyleOrSelector, processorFactory) { }
	}
}
