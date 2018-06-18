using UnityEngine;

public class BulletRewindable : ARewindable<VelocityData>
{
	private VelocityController _velocityController;
	private SpinController _spinController;
	private int _recordedUpdateCount;
	private bool _velocityHasChanged;

	public bool log;

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
		_log.OnOverrideExistingItem += ReturnDataToPool;
		RewindService.OnGhostDisappeared += HandleGhostDisappeared;
	}

	private void RemoveListeners()
	{
		_log.OnOverrideExistingItem -= ReturnDataToPool;
		RewindService.OnGhostDisappeared -= HandleGhostDisappeared;
	}

	public override void Reset()
	{
		while (!_log.IsEmpty)
		{
			DataPoolContainer.Instance.VelocityDataPool.ReturnToPool(_log.Pop());
		}

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

	private void ReturnDataToPool(VelocityData data)
	{
		if (data != null)
		{
			DataPoolContainer.Instance.VelocityDataPool.ReturnToPool(data);
		}
	}

	private void HandleGhostDisappeared()
	{
		_log.Clear();
		_recordedUpdateCount = 0;
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
			if (!_log.IsEmpty) { _log.Peek().UpdateFrameCount(1); }
			return;
		}

		var currentVelocity = TryGetCurrentVelocity();

		if (_eventQueue.Count > 0 || (!_velocityHasChanged && VelocityChangesNow(currentVelocity))
								  || _log.IsEmpty)
		{
			RecordNewDataEntry(currentVelocity);
		}
		else
		{
			_log.Peek().UpdateFrameCount(1);
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
		var newData = DataPoolContainer.Instance.VelocityDataPool.GetFromPool();

		newData.startPosition = CachedTransform.position;
		newData.velocityPerFrame = currentVelocity;
		newData.events = _eventQueue.Count < 1 ? null : _eventQueue.ToArray();
		newData.FrameCount = 1;

		_log.Push(newData);
		if (_eventQueue.Count > 0) { _eventQueue.Clear(); }
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
				DataPoolContainer.Instance.VelocityDataPool.ReturnToPool(lastRecordedData);
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