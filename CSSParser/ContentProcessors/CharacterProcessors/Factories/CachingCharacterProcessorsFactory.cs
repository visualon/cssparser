using System;
using System.Collections.Generic;
using System.Linq;

namespace CSSParser.ContentProcessors.CharacterProcessors.Factories
{
	public class CachingCharacterProcessorsFactory : IGenerateCharacterProcessors
	{
		private readonly IGenerateCharacterProcessors _processorFactory;
		private readonly Dictionary<RequestData, object> _cache;
		public CachingCharacterProcessorsFactory(IGenerateCharacterProcessors processorFactory)
		{
			if (processorFactory == null)
				throw new ArgumentNullException("processorFactory");

			_processorFactory = processorFactory;
			_cache = new Dictionary<RequestData, object>();
		}

		/// <summary>
		/// This will never return null, it will throw an exception if unable to satisfy the request
		/// </summary>
		public T Get<T>(params object[] args) where T : IProcessCharacters
		{
			if (args == null)
				throw new ArgumentNullException("args");

			var requestData = new RequestData(typeof(T), args);
			lock (_cache)
			{
				object cachedData;
				if (_cache.TryGetValue(requestData, out cachedData))
					return (T)cachedData;
			}

			var liveData = _processorFactory.Get<T>(args);
			lock (_cache)
			{
				if (!_cache.ContainsKey(requestData))
					_cache.Add(requestData, liveData);
			}
			return liveData;
		}

		public int GeneratedProcessorCount
		{
			get
			{
				lock (_cache)
				{
					return _cache.Count;
				}
			}
		}

		private sealed class RequestData
		{
			private List<object> _args;
			public RequestData(Type type, IEnumerable<object> args)
			{
				if (type == null)
					throw new ArgumentNullException("type");
				if (args == null)
					throw new ArgumentNullException("args");

				Type = type;
				_args = args.ToList();
			}

			/// <summary>
			/// This will never be null
			/// </summary>
			public Type Type { get; private set; }

			/// <summary>
			/// This will never be null but there is nothing to prevent it from containing any nulls, nor being an empty set
			/// </summary>
			public IEnumerable<object> Args { get { return _args.AsReadOnly(); } }

			public override int GetHashCode()
			{
				// We want a hash that is consistent for request with the same Type and Args values, so we'll just XOR all of the hashcodes from each
				// type / value together - that will always ensure there are never any false negatives but there shouldn't be too many false positives
				// either (which would mean the dictionary would do more Equals comparisons than necessary)
				var hash = Type.GetHashCode();
				foreach (var arg in Args)
				{
					if (arg == null)
						hash = hash ^ -1;
					else
						hash = hash ^ arg.GetHashCode();
				}
				return hash;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					throw new ArgumentNullException("obj");

				var requestData = obj as RequestData;
				if (requestData == null)
					return false;

				if ((requestData.Type != Type) || (requestData._args.Count != _args.Count))
					return false;

				for (var index = 0; index < _args.Count; index++)
				{
					var thisArg = _args[index];
					var otherArg = requestData._args[index];
					
					// If one arg is null then they both must be
					if ((thisArg == null) || (otherArg == null))
					{
						if ((thisArg != null) || (otherArg != null))
							return false;
					}

					if (!thisArg.Equals(otherArg))
						return false;
				}
				return true;
			}
		}
	}
}
