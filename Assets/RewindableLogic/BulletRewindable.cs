using UnityEngine;

public class BulletRewindable : ARewindable<VelocityData>
{
	private VelocityController _velocityController;
	private SpinController _spinController;
	private int _recordedUpdateCount;
	private bool _velocityHasChanged;

	public bool log;

	public override void Reset()
	{
		_log.Clear();
		_recordedUpdateCount = 0;

		if (_velocityController != null) { _velocityController.Stop(); }
		if (_spinController != null) { _spinController.Stop(); }

		Paused = false;
		_velocityHasChanged = false;
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		_log.Clear();
		_recordedUpdateCount = 0;
		_velocityController = velocityController;
		_spinController = spinController;

		_velocityController.Stop();
		_spinController.Stop();
		_velocityHasChanged = false;
		Paused = false;
	}

	protected override void CheckIfRewindingPossible()
	{
		HadSomethingToRewindToAtFrameStart = !_log.IsEmpty && _recordedUpdateCount > 0;
	}

	protected override void RecordData()
	{
		_recordedUpdateCount = Mathf.Min(_recordedUpdateCount + 1, LOG_SIZE_FRAMES);

		if (Paused)
		{
			_log.Peek().UpdateFrameCount(1);
			return;
		}

		var currentVelocity = TryGetCurrentVelocity();

		if (_eventQueue.Count > 0 || (!_velocityHasChanged && VelocityChangesNow(currentVelocity)))
		{
			RecordNewDataEntry(currentVelocity);
		}
		else
		{
			//TODO - so it's unsafe, but faster:
			_log.Peek().UpdateFrameCount(1);
			//UpdateLastRecordedDataEntry(deltaFrameCount: 1);
		}
	}

	private bool VelocityChangesNow(Vector3 currentVelocity)
	{
		if (!_log.IsEmpty)
		{
			var previousVelocity = _log.Peek().velocityPerFrame;
			_velocityHasChanged |= previousVelocity.x != currentVelocity.x || 
								   previousVelocity.y != currentVelocity.y ||
								   previousVelocity.z != currentVelocity.z;
			return _velocityHasChanged;
		}

		return false;
	}

	private Vector3 TryGetCurrentVelocity()
	{
		return _velocityController != null ? _velocityController.CurrentVelocityUnitsPerFrame : Vector3.zero;
	}

	private void RecordNewDataEntry(Vector3 currentVelocity)
	{
		_log.Push(new VelocityData(currentVelocity, _eventQueue, CachedTransform.position));
		_eventQueue.Clear();
	}

	private void UpdateLastRecordedDataEntry(int deltaFrameCount)
	{
		_log.UpdateLastEntry(entry => { entry.UpdateFrameCount(deltaFrameCount); return entry; });
	}

	protected override void TryApplyRecordedData()
	{
		if (!_log.IsEmpty && _recordedUpdateCount > 0)
		{
			_recordedUpdateCount -= 1;
			var lastRecordedData = _log.Peek();
			CachedTransform.position = lastRecordedData.GetCurrentPosition();
			_velocityController.CurrentVelocityUnitsPerFrame = lastRecordedData.velocityPerFrame;
			_log.Peek().UpdateFrameCount(-1);

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