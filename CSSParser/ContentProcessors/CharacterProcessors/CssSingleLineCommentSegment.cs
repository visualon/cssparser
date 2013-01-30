using System;
using CSSParser.StringNavigators;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class CssSingleLineCommentSegment : CommentSegment
	{
		private readonly IGenerateCharacterProcessors _processorFactory;
		public CssSingleLineCommentSegment(IProcessCharacters characterProcessorToReturnTo, IGenerateCharacterProcessors processorFactory) : base(characterProcessorToReturnTo)
		{
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");

			_processorFactory = processorFactory;
		}

		public override CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			// For single line comments, the line return should be considered part of the comment content (in the same way that the "/*" and "*/" sequences are
			// considered part of the content for multi-line comments)
			if (stringNavigator.TryToGetCharacterString(2) == "\r\n")
			{
				return new CharacterProcessorResult(
					CharacterCategorisationOptions.Comment,
					_processorFactory.Get<SkipCharactersSegment>(
						CharacterCategorisationOptions.Comment,
						1,
						_characterProcessorToReturnTo
					)
				);
			}
			else if ((stringNavigator.CurrentCharacter == '\r') || (stringNavigator.CurrentCharacter == '\n'))
				return new CharacterProcessorResult(CharacterCategorisationOptions.Comment, _characterProcessorToReturnTo);

			return new CharacterProcessorResult(CharacterCategorisationOptions.Comment, this);
		}
	}
}
