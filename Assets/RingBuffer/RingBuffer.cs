using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			if (_list.Count < Capacity)
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
			if (_endIndex == 0) { _endIndex = Capacity; }

			return itemToReturn;
		}

		public void Clear()
		{
			_list.Clear();
			_startIndex = 0;
			_endIndex = 0;
			IsEmpty = true;
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