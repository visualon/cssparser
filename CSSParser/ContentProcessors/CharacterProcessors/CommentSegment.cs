using System;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public abstract class CommentSegment : IProcessCharacters
	{
		protected readonly IProcessCharacters _characterProcessorToReturnTo;
		public CommentSegment(IProcessCharacters characterProcessorToReturnTo)
		{
			if (characterProcessorToReturnTo == null)
				throw new ArgumentNullException("characterProcessorToReturnTo");

			_characterProcessorToReturnTo = characterProcessorToReturnTo;
		}

		public abstract CharacterProcessorResult Process(IWalkThroughStrings stringNavigator);
	}
}
