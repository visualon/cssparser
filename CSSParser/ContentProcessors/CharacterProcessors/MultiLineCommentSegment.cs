using System;
using CSSParser.StringNavigators;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class MultiLineCommentSegment : IProcessCharacters
	{
		private readonly IGenerateCharacterProcessors _processorFactory;
		private readonly IProcessCharacters _characterProcessorToReturnTo;
		public MultiLineCommentSegment(IProcessCharacters characterProcessorToReturnTo, IGenerateCharacterProcessors processorFactory)
		{
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");
			if (characterProcessorToReturnTo == null)
				throw new ArgumentNullException("characterProcessorToReturnTo");

			_processorFactory = processorFactory;
			_characterProcessorToReturnTo = characterProcessorToReturnTo;
		}

		public CharacterProcessorResult Process(IWalkThroughStrings stringNavigator)
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
