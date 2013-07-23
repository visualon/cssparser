using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSSParser.TextReaderNavigators
{
	public class SimpleImmutableSet<T> : IEnumerable<T>
	{
		private readonly T[] _data;
		public SimpleImmutableSet() : this(new T[0]) { }
		public SimpleImmutableSet(IEnumerable<T> data) : this((data ?? new T[0]).ToArray())
		{
			if (data == null)
				throw new ArgumentNullException("data");
		}
		private SimpleImmutableSet(T[] data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			_data = data;
		}

		public int Count
		{
			get { return _data.Length; }
		}

		public SimpleImmutableSet<T> Add(T value)
		{
			var newData = new T[_data.Length + 1];
			if (_data.Length > 0)
				Array.Copy(_data, newData, _data.Length);
			newData[newData.Length - 1] = value;
			return new SimpleImmutableSet<T>(newData);
		}

		public SimpleImmutableSet<T> AddRange(IEnumerable<T> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");

			var valuesArray = (values as T[]) ?? values.ToArray();
			if (valuesArray.Length == 0)
				return this;

			var newData = new T[_data.Length + valuesArray.Length];
			if (_data.Length > 0)
				Array.Copy(_data, newData, _data.Length);
			Array.Copy(valuesArray, 0, newData, _data.Length, valuesArray.Length);
			return new SimpleImmutableSet<T>(newData);
		}

		public SimpleImmutableSet<T> RemoveAt(int index)
		{
			if ((index < 0) || (index >= _data.Length))
				throw new ArgumentOutOfRangeException("index");

			var newData = new T[_data.Length - 1];
			if (index > 0)
				Array.Copy(_data, newData, index);
			if (index < (_data.Length - 1))
				Array.Copy(_data, index + 1, newData, index, _data.Length - (index + 1));
			return new SimpleImmutableSet<T>(newData);
		}

		public SimpleImmutableSet<T> RemoveWhere(Predicate<T> condition)
		{
			if (condition == null)
				throw new ArgumentNullException("condition");

			return new SimpleImmutableSet<T>(_data.Where(value => !condition(value)).ToArray());
		}

		public IEnumerator<T> GetEnumerator() { return ((IEnumerable<T>)_data).GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return _data.GetEnumerator(); }
	}
}
