using System.Collections.Generic;
using CSSParser.ContentProcessors.CharacterProcessors;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.StringProcessors
{
	/// <summary>
	/// This will use IWalkThroughStrings and IProcessCharacters to generate a set of CategorisedCharacterString instances
	/// </summary>
	public interface ICollectStringsOfProcessedCharacters
	{
		/// <summary>
		/// This will never return null nor a set containing any null references. It will throw an exception for null contentWalker or contentProcessor
		/// references or it the processing failed.
		/// </summary>
		IEnumerable<CategorisedCharacterString> GetStrings(IWalkThroughStrings contentWalker, IProcessCharacters contentProcessor);
	}
}
