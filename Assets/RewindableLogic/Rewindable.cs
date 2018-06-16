//TODO: name this a bit differently, to indicate that this guy
//		doesn't do anything fancy, it just records and replays
//		transform data, without trying to be clever about it
public class Rewindable : ARewindable<TransformData>
{
	public override void Reset()
	{
		while (!_log.IsEmpty)
		{
			TransformDataPool.Instance.Pool.ReturnToPool(_log.Pop());
		}

		//TODO: So what would be nicer would be a "set to null",
		//		so that we can add in place; but at least this
		//		seems to fix one of the issues
		_log.Clear(true);
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		_log.Clear();
		_log.OnOverrideExistingItem -= ReturnItemToPool;
		_log.OnOverrideExistingItem += ReturnItemToPool;
	}

	private void ReturnItemToPool(TransformData data)
	{
		TransformDataPool.Instance.Pool.ReturnToPool(data);
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var trData = _log.Pop();
			CachedTransform.position = trData.position;
			CachedTransform.rotation = trData.rotation;

			if (trData.events != null && trData.events.Length > 0)
			{
				foreach (var evt in trData.events)
				{
					evt.Apply(isRewind: true);
				}
			}

			TransformDataPool.Instance.Pool.ReturnToPool(trData);
		}
	}

	protected override void RecordData()
	{
		var newData = TransformDataPool.Instance.Pool.GetFromPool();

		if (newData == null)
		{
			newData = new TransformData(CachedTransform.position, CachedTransform.rotation, _eventQueue, -1);
		}
		else
		{
			newData.position = CachedTransform.position;
			newData.rotation = CachedTransform.rotation;
			newData.events = _eventQueue.Count < 1 ? null : _eventQueue.ToArray();
		}

		_log.Push(newData);
		_eventQueue.Clear();
	}
}
