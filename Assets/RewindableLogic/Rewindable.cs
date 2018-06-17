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

		_log.Clear();
		Paused = false;
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		_log.Clear();
		_log.OnOverrideExistingItem -= ReturnItemToPool;
		_log.OnOverrideExistingItem += ReturnItemToPool;
		Paused = false;
	}

	private void ReturnItemToPool(TransformData data)
	{
		if (data != null)
		{
			TransformDataPool.Instance.Pool.ReturnToPool(data);
		}
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var trData = _log.Pop();

			if (trData == null) { return; }

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
		if (Paused) { _log.Push(null); return; }

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
