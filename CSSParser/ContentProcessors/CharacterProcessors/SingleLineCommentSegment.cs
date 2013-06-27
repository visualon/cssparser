using System;
using CSSParser.StringNavigators;
using CSSParser.ContentProcessors.CharacterProcessors.Factories;

namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public class SingleLineCommentSegment : IProcessCharacters
	{
		private readonly IGenerateCharacterProcessors _processorFactory;
		private readonly IProcessCharacters _characterProcessorToReturnTo;
		public SingleLineCommentSegment(IProcessCharacters characterProcessorToReturnTo, IGenerateCharacterProcessors processorFactory)
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
