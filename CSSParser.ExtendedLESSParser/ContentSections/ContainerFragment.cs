using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSSParser.ExtendedLESSParser.ContentSections
{
	/// <summary>
	/// This is a CSS fragment that represents contains other fragments - eg. a Selector or Media Query
	/// </summary>
	public abstract class ContainerFragment : ICSSFragment
	{
		private readonly List<SelectorSet> _parentSelectors;
		private readonly List<ICSSFragment> _childFragments;
		public ContainerFragment(
			SelectorSet selectors,
			IEnumerable<SelectorSet> parentSelectors,
			int sourceLineIndex,
			IEnumerable<ICSSFragment> childFragments)
		{
			if (selectors == null)
				throw new ArgumentNullException("selectors");
			if (parentSelectors == null)
				throw new ArgumentNullException("parentSelectors");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");
			if (childFragments == null)
				throw new ArgumentNullException("childFragments");

			var parentSelectorsTidied = parentSelectors.ToList();
			if (parentSelectorsTidied.Any(s => s == null))
				throw new ArgumentException("Null reference encountered in parentSelectors set");

			var childFragmentsTidied = childFragments.ToList();
			if (childFragmentsTidied.Any(f => f == null))
				throw new ArgumentException("Null reference encountered in childFragments set");

			Selectors = selectors;
			SourceLineIndex = sourceLineIndex;
			_parentSelectors = parentSelectorsTidied;
			_childFragments = childFragmentsTidied;
		}

		/// <summary>
		/// This will never be null, empty nor contain any nulls
		/// </summary>
		public SelectorSet Selectors { get; private set; }

		/// <summary>
		/// This will never be null nor contain any nulls, it may be empty if this is a top-level Selector
		/// </summary>
		public IEnumerable<SelectorSet> ParentSelectors { get { return _parentSelectors.AsReadOnly(); } }

		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		public int SourceLineIndex { get; private set; }

		/// <summary>
		/// This will never be null nor contain any nulls, it may be empty if there were no child fragments for the Selector
		/// </summary>
		public IEnumerable<ICSSFragment> ChildFragments { get { return _childFragments.AsReadOnly(); } }

		public override string ToString()
		{
			return base.ToString() + ":" + string.Join(", ", Selectors.Select(s => s.Value));
		}

		/// <summary>
		/// The set will always contain at least one item and none of them will be null references 
		/// </summary>
		public class SelectorSet : IEnumerable<WhiteSpaceNormalisedString>
		{
			private List<WhiteSpaceNormalisedString> _selectors;
			public SelectorSet(IEnumerable<WhiteSpaceNormalisedString> selectors)
			{
				if (selectors == null)
					throw new ArgumentNullException("selectors");

				var selectorsTidied = selectors.ToList();
				if (selectors.Any(s => s == null))
					throw new ArgumentException("Null reference encountered in selectors set");
				if (selectors.Any(s => s.Value.Contains(",")))
					throw new ArgumentException("Specified selectors set contains at least one entry containing a comma, selectors must be broken on commas");
				if (!selectors.Any())
					throw new ArgumentException("Empty selectors set specified");

				_selectors = selectorsTidied;
			}

			/// <summary>
			/// The enumerated data will never be empty nor contain any null references
			/// </summary>
			public IEnumerator<WhiteSpaceNormalisedString> GetEnumerator()
			{
				return _selectors.GetEnumerator();
			}
			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class WhiteSpaceNormalisedString
		{
			public WhiteSpaceNormalisedString(string value)
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentException("Null/blank value specified");

				var valueTidied = new string(value.Select(c => char.IsWhiteSpace(c) ? ' ' : c).ToArray());
				while (valueTidied.Contains("  "))
					valueTidied = valueTidied.Replace("  ", " ");
				valueTidied = valueTidied.Trim();

				Value = valueTidied;
			}

			/// <summary>
			/// All whitespace characters will have been replaced with spaces, then any runs of spaces replaced with single instances and finally
			/// the string will have been trimmed. This will always have some content and never be null or blank.
			/// </summary>
			public string Value { get; private set; }

			public override string ToString()
			{
				return base.ToString() + ":" + Value;
			}
		}
	}
}
