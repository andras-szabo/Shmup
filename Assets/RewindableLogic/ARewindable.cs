using System.Collections.Generic;
using RingBuffer;

public abstract class ABaseRewindable : MonoWithCachedTransform, IRewindable
{
	public bool IsRewinding { get; protected set; }
	public bool HadSomethingToRewindToAtFrameStart { get; protected set; }
	public int LogCount { get; protected set; }
	public bool Paused { get; set; }

	protected bool AlwaysRecord { get; set; }

	public abstract void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false);
	public abstract void Init(VelocityController velocityController, SpinController spinController);
	public abstract void Reset();
}

public abstract class ARewindable<T> : ABaseRewindable
{
	public const int LOG_SIZE_FRAMES = 200;

	protected List<IRewindableEvent> _eventQueue = new List<IRewindableEvent>();
	protected RingBuffer<T> _log = new RingBuffer<T>(LOG_SIZE_FRAMES, cleanupOnPop: true);

	private RewindableService _rewindService;
	protected RewindableService RewindService { get { return _rewindService ?? (_rewindService = RewindableService.Instance); } }

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
			if (AlwaysRecord || RewindService.IsRecordingAllowed)
			{
				RecordData();
			}
		}
	}

	protected virtual void CheckIfRewindingPossible()
	{
		HadSomethingToRewindToAtFrameStart = !_log.IsEmpty;
	}

	private void CheckIfRewindingRequested()
	{
		IsRewinding = RewindService.ShouldRewind;
	}
}