using System.Collections.Generic;
using RingBuffer;
using UnityEngine;

public abstract class ARewindable<T> : MonoWithCachedTransform, IRewindable
{
	public const int LOG_SIZE_FRAMES = 128;

	public bool IsRewinding { get; private set; }
	public bool HadSomethingToRewindToAtFrameStart { get; private set; }
	public int LogCount { get; private set; }

	protected List<IRewindableEvent> _eventQueue = new List<IRewindableEvent>();
	protected RingBuffer<T> _log = new RingBuffer<T>(LOG_SIZE_FRAMES);

	public void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false)
	{
		_eventQueue.Add(evt);
		if (recordImmediately) { RecordData(); }
	}

	protected abstract void RecordData();
	protected abstract void TryApplyRecordedData();

	//Shit, this could also go to ARewindable
	private void FixedUpdate()
	{
		CheckIfRewindingRequested();
		CheckIfRewindingPossible();
		if (IsRewinding)
		{
			if (!_log.IsEmpty)
			{
				TryApplyRecordedData();
			}
		}
		else
		{
			RecordData();
		}
	}

	private void CheckIfRewindingPossible()
	{
		HadSomethingToRewindToAtFrameStart = !_log.IsEmpty;
	}

	private void CheckIfRewindingRequested()
	{
		IsRewinding = InputController.Instance.IsHoldingDoubleTap;
	}
}