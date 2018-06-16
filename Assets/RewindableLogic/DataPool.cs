using System.Collections.Generic;

public interface IDataPoolable
{
	int IndexInPool { get; set; }
}

//TODO: KP multi constraints on same type, order matters
public class DataPool<T> where T : IDataPoolable, new()
{
	private Stack<int> _available;
	private HashSet<int> _availableIndices;
	private List<T> _pool;

	public int AvailableCount { get { return _available.Count; } }

	public bool IsIndexAvailable(int soughtIndex)
	{
		foreach (var index in _available)
		{
			if (soughtIndex == index)
			{
				return true;
			}
		}

		return false;
	}

	public DataPool(int capacity)
	{
		CreatePool(capacity);
	}

	private void CreatePool(int capacity)
	{
		_pool = new List<T>(capacity);
		_available = new Stack<int>(capacity);
		_availableIndices = new HashSet<int>();

		for (int i = 0; i < capacity; ++i)
		{
			var item = new T();
			item.IndexInPool = i;
			_pool.Add(item);
			_available.Push(i);
			_availableIndices.Add(i);
		}
	}

	public T GetFromPool()
	{
		if (_available.Count < 1)
		{
			throw new System.IndexOutOfRangeException("DataPool ran out of pooled members.");
		}

		var index = _available.Pop();
		_availableIndices.Remove(index);
		return _pool[index];
	}

	public void ReturnToPool(T item)
	{
		if (!_availableIndices.Contains(item.IndexInPool))
		{
			_availableIndices.Add(item.IndexInPool);
			_available.Push(item.IndexInPool);
		}
	}
}
