using UnityEngine;

public class SimpleBulletRewindable : ABaseRewindable
{
	private RewindableService _rewindService;
	protected RewindableService RewindService { get { return _rewindService ?? (_rewindService = RewindableService.Instance); } }

	private Vector3 _startPosition;
	private Vector3 _velocityPerFrame;
	private int _recordedUpdateCount;
	private int _frameCount;

	private IRewindableEvent _despawnOnRewindEvent;

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
		RewindService.OnGhostDisappeared += Reset;
	}

	private void RemoveListeners()
	{
		RewindService.OnGhostDisappeared -= Reset;
	}

	private void FixedUpdate()
	{
		IsRewinding = RewindService.ShouldRewind;
		CheckIfRewindingPossible();
		if (IsRewinding)
		{
			TryApplyRecordedData();
		}
		else
		{
			if (AlwaysRecord || RewindService.IsRecordingAllowed)
			{
				RecordData();
			}
		}
	}

	private void CheckIfRewindingPossible()
	{
		HadSomethingToRewindToAtFrameStart = _recordedUpdateCount > 0;
	}

	public const int LOG_SIZE_FRAMES = 200;

	private void RecordData()
	{
		if (_recordedUpdateCount < LOG_SIZE_FRAMES) { ++_recordedUpdateCount; }
		_frameCount++;
	}

	private void TryApplyRecordedData()
	{
		if (_recordedUpdateCount > 0 && _frameCount > 0)
		{
			CachedTransform.position = _startPosition + (_velocityPerFrame * _frameCount--);
			if (--_recordedUpdateCount == 0)
			{
				_despawnOnRewindEvent.Apply(isRewind: true);
			}
		}
	}

	public override void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false)
	{
		if (recordImmediately) { _despawnOnRewindEvent = evt; }
		else { throw new System.Exception("Bullet has no idea what to do with some event."); }
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		_startPosition = CachedTransform.position;
		_velocityPerFrame = velocityController.CurrentVelocityUnitsPerFrame;
	}

	public override void Reset()
	{
		_recordedUpdateCount = 0;
		_frameCount = 0;
	}
}