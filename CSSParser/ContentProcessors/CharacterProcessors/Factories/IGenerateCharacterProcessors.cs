using System;

namespace CSSParser.ContentProcessors.CharacterProcessors.Factories
{
	public interface IGenerateCharacterProcessors
	{
		/// <summary>
		/// This will never return null, it will throw an exception if unable to satisfy the request
		/// </summary>
		T Get<T>(params object[] args) where T : IProcessCharacters;
	}
}
