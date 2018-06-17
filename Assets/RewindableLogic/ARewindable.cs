using System.Collections.Generic;
using RingBuffer;

public abstract class ABaseRewindable : MonoWithCachedTransform, IRewindable
{
	public bool IsRewinding { get; protected set; }
	public bool HadSomethingToRewindToAtFrameStart { get; protected set; }
	public int LogCount { get; protected set; }
	public bool Paused { get; set; }

	public abstract void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false);
	public abstract void Init(VelocityController velocityController, SpinController spinController);
	public abstract void Reset();
}

public abstract class ARewindable<T> : ABaseRewindable
{
	public const int LOG_SIZE_FRAMES = 200;

	protected List<IRewindableEvent> _eventQueue = new List<IRewindableEvent>();
	protected RingBuffer<T> _log = new RingBuffer<T>(LOG_SIZE_FRAMES, cleanupOnPop: true);
	protected InputController _inputController;

	public override void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false)
	{
		_eventQueue.Add(evt);
		if (recordImmediately) { RecordData(); }
	}

	protected abstract void RecordData();
	protected abstract void TryApplyRecordedData();

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
		if (_inputController == null) { _inputController = InputController.Instance; }
		IsRewinding = _inputController.IsHoldingDoubleTap && !Ghost.IsReplaying;
	}
}