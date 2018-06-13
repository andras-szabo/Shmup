using UnityEngine;

public class BulletRewindable : ARewindable<VelocityData>
{
	private VelocityController _velocityController;
	private SpinController _spinController;

	public void Reset(VelocityController velocityController, SpinController spinController)
	{
		_log.Clear();
		_velocityController = velocityController;
		_spinController = spinController;

		_velocityController.Stop();
		_spinController.Stop();
	}

	protected override void RecordData()
	{
		var currentVelocity = _velocityController.CurrentVelocityUnitsPerFrame;
		var currentSpin = _spinController.RotationPerFrame;

		if (_eventQueue.Count > 0 || ShouldRecordNewEntry(currentVelocity, currentSpin))
		{
			RecordNewDataEntry(currentVelocity, currentSpin);
		}
		else
		{
			UpdateLastRecordedDataEntry(deltaFrameCount: 1);
		}
	}

	private void RecordNewDataEntry(Vector3 currentVelocity, Vector3 currentSpin)
	{
		_log.Push(new VelocityData(currentVelocity, currentSpin, _eventQueue,
								   CachedTransform.position, CachedTransform.rotation));
		_eventQueue.Clear();
	}

	private void UpdateLastRecordedDataEntry(int deltaFrameCount)
	{
		_log.UpdateLastEntry(entry => { entry.UpdateFrameCount(deltaFrameCount); return entry; });
	}

	private bool ShouldRecordNewEntry(Vector3 currentVelocity, Vector3 currentSpin)
	{
		var shouldRecordNewEntry = true;

		if (!_log.IsEmpty)
		{
			var lastRecordedEntry = _log.Peek();
			shouldRecordNewEntry = IsDifferent(currentVelocity, lastRecordedEntry.velocityPerFrame) ||
								   IsDifferent(currentSpin, lastRecordedEntry.spinPerFrame);
		}

		return shouldRecordNewEntry;
	}

	private bool IsDifferent(Vector3 a, Vector3 b)
	{
		return !Mathf.Approximately((a.x - b.x) + (a.y - b.y) + (a.z - b.z), 0f);
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty)
		{
			var lastRecordedData = _log.Peek();
			CachedTransform.position = lastRecordedData.GetCurrentPosition();
			CachedTransform.rotation = lastRecordedData.GetCurrentRotation();

			_velocityController.CurrentVelocityUnitsPerFrame = lastRecordedData.velocityPerFrame;
			_spinController.RotationPerFrame = lastRecordedData.spinPerFrame;

			_log.UpdateLastEntry(entry => { entry.UpdateFrameCount(-1); return entry; });

			if (_log.Peek().FrameCount == 0)
			{
				lastRecordedData = _log.Pop();
				ApplyRecordedEvents(lastRecordedData);
			}
		}
	}

	private void ApplyRecordedEvents(VelocityData lastRecordedData)
	{
		if (lastRecordedData.events != null && lastRecordedData.events.Length > 0)
		{
			foreach (var evt in lastRecordedData.events)
			{
				evt.Apply(isRewind: true);
			}
		}
	}
}