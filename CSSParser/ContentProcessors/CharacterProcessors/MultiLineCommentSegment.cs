using System;
using CSSParser.StringNavigators;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class MultiLineCommentSegment : CommentSegment
	{
		private readonly IGenerateCharacterProcessors _processorFactory;
		public MultiLineCommentSegment(IProcessCharacters characterProcessorToReturnTo, IGenerateCharacterProcessors processorFactory) : base(characterProcessorToReturnTo)
		{
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");

			_processorFactory = processorFactory;
		}

		public override CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
		{
			if (stringNavigator == null)
				throw new ArgumentNullException("stringNavigator");

			if ((stringNavigator.CurrentCharacter == '*') && (stringNavigator.Next.CurrentCharacter == '/'))
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
			return new CharacterProcessorResult(
				CharacterCategorisationOptions.Comment,
				this
			);
		}
	}
}
