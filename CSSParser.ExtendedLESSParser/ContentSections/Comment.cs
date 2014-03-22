using System;

namespace CSSParser.ExtendedLESSParser.ContentSections
{
	/// <summary>
	/// The Content property contains the comment control characters (whether double-slash or slash-star and star-slash) but it will not include line returns,
	/// even if this is a single-line comment (for which, arguably, the line return is part of the control characters)
	/// </summary>
	public class Comment : ICSSFragment
	{
		public Comment(string content, int sourceLineIndex)
		{
			if (string.IsNullOrWhiteSpace(content))
				throw new ArgumentException("Null/blank content specified");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");

			Content = content.Trim();
			SourceLineIndex = sourceLineIndex;
		}

		/// <summary>
		/// This will never be null or blank
		/// </summary>
		public string Content { get; private set; }

		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		public int SourceLineIndex { get; private set; }

		public override string ToString()
		{
			return base.ToString() + ":" + ((Content.Length > 30) ? (Content.Substring(0, 25) + "..") : Content);
		}
	}
}
