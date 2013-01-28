namespace CSSParser.ContentProcessors.CharacterProcessors
{
	public interface ISegment
	{
		/// <summary>
		/// This will never be null
		/// </summary>
		string Content { get; }
	}
}
