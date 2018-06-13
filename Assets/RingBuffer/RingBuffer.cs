using System.Collections.Generic;

namespace RingBuffer
{
	public class RingBuffer<T>
	{
		public int Capacity { get; protected set; }
		public bool IsEmpty { get; protected set; }

		public int Count
		{
			get
			{
				if (IsEmpty) { return 0; }
				if (_endIndex > _startIndex) {	return _endIndex - _startIndex; }
				if (IsEmpty || _endIndex == 0) { return 0; }
				return Capacity - (_endIndex - _startIndex);
			}
		}

		protected List<T> _list;
		protected int _startIndex;
		protected int _endIndex;

		public RingBuffer(int size)
		{
			Capacity = size;
			_list = new List<T>(size);
			IsEmpty = true;
		}

		public void Push(T newItem)
		{
			if (_list.Count < Capacity && _endIndex == _list.Count)
			{
				_list.Add(newItem);
				_endIndex += 1;
			}
			else
			{
				var newLastIndexCandidate = FindInsertionIndex();
				_list[newLastIndexCandidate] = newItem;
				_endIndex = newLastIndexCandidate + 1;

				if (!IsEmpty && newLastIndexCandidate == _startIndex)
				{
					IncrementStartIndex();
				}
			}

			IsEmpty = false;
		}
			
		public T Pop()
		{
			if (IsEmpty) { throw new System.ArgumentException("Trying to pop from empty ringbuffer."); }

			var lastIndex = _endIndex - 1;
			var itemToReturn = _list[lastIndex];

			if (lastIndex == _startIndex) { IsEmpty = true; }

			_endIndex -= 1;
			if (_endIndex == 0) { _endIndex = _list.Count; }
			
			return itemToReturn;
		}

		public T Peek()
		{
			if (IsEmpty) { throw new System.ArgumentException("Trying to peek into empty ringbuffer."); }
			return _list[_endIndex - 1];
		}

		public void UpdateLastEntry(System.Func<T, T> updateMethod)
		{
			if (IsEmpty) { throw new System.ArgumentException("Trying to update last entry in empty ringbuffer."); }

			if (updateMethod != null)
			{
				var lastIndex = _endIndex - 1;
				_list[lastIndex] = updateMethod(_list[lastIndex]);
			}

		}

		public void Clear()
		{
			_startIndex = (_list.Count < Capacity) ? _list.Count : 0;
			_endIndex = _startIndex;
			IsEmpty = true;
		}

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();

			sb.AppendFormat("Start: {0}, end: {1}, count: {2}\n", _startIndex, _endIndex, Count);
			foreach (var item in _list)
			{
				sb.AppendFormat("[{0}] ", item.ToString());
			}

			return sb.ToString();
		}

		protected void IncrementStartIndex()
		{
			if (++_startIndex == Capacity)
			{
				_startIndex = 0;
			}
		}

		protected int FindInsertionIndex()
		{
			var newLastIndexCandidate = _endIndex;
			if (newLastIndexCandidate == Capacity)
			{
				newLastIndexCandidate = 0;
			}
			return newLastIndexCandidate;
		}
	}
}