using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public interface IProcessCharacters
	{
		CharacterProcessorResult Process(IWalkThroughStrings stringNavigator);
	}
}
