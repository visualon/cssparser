using System;
using CSSParser.StringNavigators;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class CssSingleLineCommentSegment : CommentSegment
	{
		public CssSingleLineCommentSegment(IProcessCharacters characterProcessorToReturnTo) : base(characterProcessorToReturnTo) { }

		public override CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			if ((stringNavigator.CurrentCharacter == '\r') || (stringNavigator.CurrentCharacter == '\n'))
				return new CharacterProcessorResult(CharacterCategorisationOptions.Comment, _characterProcessorToReturnTo);

			return new CharacterProcessorResult(CharacterCategorisationOptions.Comment, this);
		}
	}
}
