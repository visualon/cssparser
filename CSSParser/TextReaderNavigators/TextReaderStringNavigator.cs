using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSSParser.StringNavigators;

namespace CSSParser.TextReaderNavigators
{
	public class TextReaderStringNavigator : IWalkThroughStrings
	{
		private readonly TextReaderWithReadAheadEvent _reader;
		private readonly int _position;
		private readonly List<char> _catchUpQueue;

		public TextReaderStringNavigator(TextReader reader) : this(new TextReaderWithReadAheadEvent(reader), 0, new List<char>()) { }

		/// <summary>
		/// All instances returned from requests to the Next property will share the same TextReaderWithReadAheadEvent instance and use this as the
		/// synchronisation object when any operations that must be thread safe will occur. This includes any calls to Read since it must be guaranteed
		/// that each Read call can transmit the read character(s) to any TextReaderStringNavigator instances that are lagging behind to ensure that
		/// their catchUpQueue is complete. A lock must also be obtained any time that a new TextReaderStringNavigator is created that shares the
		/// TextReaderWithReadAheadEvent instance since its constructor will register with the ReadAhead event and mustn't miss any data from Read requests
		/// that may occur during the class' instantiation. By extension a lock must be obtained any time that the catchUpQueue is accessed (either to
		/// retrieve data or to change the contents) since its contents may be changed where the ReadAhead subscription is processed which may be
		/// across multiple threads. The catchUpQueue's purpose is to allow instances of TextReaderStringNavigator that lag behind the wrapped
		/// TextReaderWithReadAheadEvent to return CurrentCharacter data without having to query the TextReaderWithReadAheadEvent (which won't be able
		/// to help since it has progressed past the TextReaderStringNavigator's position). This mechanism can't prevent the source TextReader from being
		/// progressed by another object with a reference to the TextReader but it does guarantee that all of the TextReaderStringNavigator instances will
		/// have data consistent with the other TextReaderStringNavigator instances at all times. The TextReaderWithReadAheadEvent being private is an
		/// extra layer of insurance that its Read method will not be called without the proper locks in place and should help ensure that a reference
		/// to TextReaderWithReadAheadEvents can not be leaked out which could risk deadlocks as they are used as the synchronisation object.
		/// </summary>
		private TextReaderStringNavigator(TextReaderWithReadAheadEvent reader, int position, List<char> catchUpQueue)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			if (catchUpQueue == null)
				throw new ArgumentNullException("catchUpQueue");
			if (position < (reader.Position - catchUpQueue.Count))
				throw new ArgumentOutOfRangeException("position", "may not be less than the reader's Position minus the catchUpQueue's length");

			_reader = reader;
			_position = position;
			_catchUpQueue = catchUpQueue;

			_reader.ReadAhead += (sender, e) =>
			{
				// Although the catchUpQueue is being altered here, no lock on the reader must be explicitly obtained at this point since the ReadAhead
				// event is only raised when the Reader's Read method is called which only happens when the CurrentCharacter property of this class is
				// accessed, at which point a lock on the reader has already been obtained. This means that only a single thread at a time can retrieve
				// data from any set of TextReaderStringNavigator instances that share the same TextReaderWithReadAheadEvent reference but it guarantees
				// their consistency.
				if (e.FromPosition >= _position)
					_catchUpQueue.Add(e.Character);
			};
		}

		/// <summary>
		/// This return null if the current location in the string has no content (eg. anywhere on an empty string or past the end of a non-empty string)
		/// </summary>
		public char? CurrentCharacter
		{
			get
			{
				// This lock is required since the catchUpQueue is about to be accessed and this access must be thread-safe (must ensure that no other
				// string navigator that shares the TextReaderWithReadAheadEvent is simultaneously entering the following section and calling the Read
				// method which may then potentially change the current instance's catchUpQueue from being empty to being populated)
				lock (_reader)
				{
					if (_catchUpQueue.Count > 0)
						return _catchUpQueue[0];

					// This TextReaderStringNavigator's position value may be ahead of the TextReaderWithReadAheadEvent's Position and so multiple calls
					// to its Read method may be required to get the CurrentCharacter for this instance. It's also possible that this string navigator
					// has been moved passed the end of the available content, in which case the Read method will return null and this property's value
					// should also be returned as null.
					while ((_reader.Position <= _position) && (_reader.Read() != null)) { }
				}
				return (_catchUpQueue.Count == 0) ? (char?)null : _catchUpQueue[0];
			}
		}

		/// <summary>
		/// This will never return null
		/// </summary>
		public IWalkThroughStrings Next
		{
			get
			{
				// As described by the comment in the constructor, we need to obtain a lock on the reader in order to instantiate a new TextReaderStringNavigator
				// that shares the reader reference (which we do in order to provide a string navigator that represents the next character in the content)
				lock (_reader)
				{
					return new TextReaderStringNavigator(
						_reader,
						_position + 1,
						_catchUpQueue.Skip(1).ToList()
					);
				}
			}
		}

		/// <summary>
		/// This will try to extract a string of length requiredNumberOfCharacters from the current position in the string navigator. If there are insufficient
		/// characters available, then a string containing all of the remaining characters will be returned. This will be an empty string if there is no more
		/// content to deliver. This will never return null.
		/// </summary>
		public string TryToGetCharacterString(int requiredNumberOfCharacters)
		{
			if (requiredNumberOfCharacters <= 0)
				throw new ArgumentOutOfRangeException("requiredNumberOfCharacters", "must be greater than zero");

			var characters = new List<char>();
			var stringNavigator = (IWalkThroughStrings)this;
			while (characters.Count < requiredNumberOfCharacters)
			{
				var character = stringNavigator.CurrentCharacter;
				if (character == null)
					break;
				characters.Add(character.Value);
				stringNavigator = stringNavigator.Next;
			}
			return new string(characters.ToArray());
		}

		private class TextReaderWithReadAheadEvent
		{
			private readonly TextReader _reader;
			private readonly object _readAheadLock;
			public TextReaderWithReadAheadEvent(TextReader reader)
			{
				if (reader == null)
					throw new ArgumentNullException("reader");

				_reader = reader;
				_readAheadLock = new object();
				Position = 0;
			}

			private WeakEventSource<ReadAheadEventArgs> _readAhead;
			public event EventHandler<ReadAheadEventArgs> ReadAhead
			{
				add { WeakEventSourceThreadSafeOperations.Add(ref _readAhead, value); }
				remove { WeakEventSourceThreadSafeOperations.Remove(ref _readAhead, value); }
			}
			private void OnReadAhead(ReadAheadEventArgs e)
			{
				if (e == null)
					throw new ArgumentNullException("e");

				WeakEventSourceThreadSafeOperations.Fire(_readAhead, this, e);
			}

			public char? Read()
			{
				var buffer = new char[1];
				var numberOfCharactersRead = _reader.Read(buffer, 0, 1);
				if (numberOfCharactersRead == 0)
					return null;

				var character = buffer[0];
				OnReadAhead(new ReadAheadEventArgs(character, Position));
				Position++;
				return character;
			}

			public int Position { get; private set; }

			public class ReadAheadEventArgs : EventArgs
			{
				public ReadAheadEventArgs(char character, int fromPosition)
				{
					if (fromPosition < 0)
						throw new ArgumentOutOfRangeException("fromPosition", "must be zero or greater");

					FromPosition = fromPosition;
					Character = character;
				}

				public char Character { get; private set; }
				
				/// <summary>
				/// This will always be zero or greater
				/// </summary>
				public int FromPosition { get; private set; }
			}
		}
	}
}
