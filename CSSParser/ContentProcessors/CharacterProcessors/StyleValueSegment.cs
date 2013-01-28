using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class StyleValueSegment : SelectorOrStyleSegment
	{
		public StyleValueSegment(IGenerateCharacterProcessors processorFactory) : base(ProcessingTypeOptions.Value, processorFactory) { }
	}
}
