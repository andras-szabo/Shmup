//TODO: name this a bit differently, to indicate that this guy
//		doesn't do anything fancy, it just records and replays
//		transform data, without trying to be clever about it
public class Rewindable : ARewindable<TransformData>
{
	public bool ignorePositionUpdates;
	public bool ignoreRotationUpdates;

	private void Awake()
	{
		AddListeners();
	}

	private void OnDestroy()
	{
		RemoveListeners();
	}

	private void AddListeners()
	{
		_log.OnOverrideExistingItem += ReturnItemToPool;
		RewindService.OnGhostDisappeared += HandleGhostDisappeared;
	}

	private void RemoveListeners()
	{
		_log.OnOverrideExistingItem -= ReturnItemToPool;
		RewindService.OnGhostDisappeared -= HandleGhostDisappeared;
	}

	public override void Reset()
	{
		while (!_log.IsEmpty)
		{
			DataPoolContainer.Instance.TransformDataPool.ReturnToPool(_log.Pop());
		}

		_log.Clear();
		Paused = false;
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		_log.Clear();
		Paused = false;
	}

	private void HandleGhostDisappeared()
	{
		_log.Clear();
	}

	private void ReturnItemToPool(TransformData data)
	{
		if (data != null)
		{
			DataPoolContainer.Instance.TransformDataPool.ReturnToPool(data);
		}
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var trData = _log.Pop();

			if (trData == null) { return; }

			if (!ignorePositionUpdates) { CachedTransform.position = trData.position; }
			if (!ignoreRotationUpdates) { CachedTransform.rotation = trData.rotation; }

			if (trData.events != null && trData.events.Length > 0)
			{
				foreach (var evt in trData.events)
				{
					evt.Apply(isRewind: true);
				}
			}

			DataPoolContainer.Instance.TransformDataPool.ReturnToPool(trData);
		}
	}

	protected override void RecordData()
	{
		if (Paused) { _log.Push(null); return; }

		var newData = DataPoolContainer.Instance.TransformDataPool.GetFromPool();

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

		if (_eventQueue.Count > 0) { _eventQueue.Clear(); }
	}
}
