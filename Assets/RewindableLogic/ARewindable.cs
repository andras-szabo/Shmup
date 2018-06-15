using System.Collections.Generic;
using RingBuffer;

public abstract class ABaseRewindable : MonoWithCachedTransform, IRewindable
{
	public bool IsRewinding { get; protected set; }
	public bool HadSomethingToRewindToAtFrameStart { get; protected set; }
	public int LogCount { get; protected set; }

	public abstract void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false);
	public abstract void Init(VelocityController velocityController, SpinController spinController);
	public abstract void Reset();
}

public abstract class ARewindable<T> : ABaseRewindable
{
	public const int LOG_SIZE_FRAMES = 128;

	protected List<IRewindableEvent> _eventQueue = new List<IRewindableEvent>();
	protected RingBuffer<T> _log = new RingBuffer<T>(LOG_SIZE_FRAMES);

	public override void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false)
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

	protected virtual void CheckIfRewindingPossible()
	{
		HadSomethingToRewindToAtFrameStart = !_log.IsEmpty;
	}

	private void CheckIfRewindingRequested()
	{
		IsRewinding = InputController.Instance.IsHoldingDoubleTap;
	}
}