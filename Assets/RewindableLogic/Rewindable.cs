//TODO: name this a bit differently, to indicate that this guy
//		doesn't do anything fancy, it just records and replays
//		transform data, without trying to be clever about it
public class Rewindable : ARewindable<TransformData>
{
	public void Reset()
	{
		_log.Clear();
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
		}
	}

	protected override void RecordData()
	{
		_log.Push(new TransformData(CachedTransform.position, CachedTransform.rotation, _eventQueue));
		_eventQueue.Clear();
	}
}
