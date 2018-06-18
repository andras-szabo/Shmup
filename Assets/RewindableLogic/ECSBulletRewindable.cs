public class ECSBulletRewindable : ABaseRewindable
{
	private RewindableService _rewindService;
	protected RewindableService RewindService { get { return _rewindService ?? (_rewindService = RewindableService.Instance); } }

	private TransformSystem _transformSystem;
	private TransformSystem TransformSystem { get { return _transformSystem ?? (_transformSystem = TransformSystem.Instance); } }

	private IRewindableEvent _despawnOnRewindEvent;
	private int _myIndex = -1;
	private VelocityController _velocityController;

	private void FixedUpdate()
	{
		IsRewinding = RewindService.ShouldRewind;
		var updateCount = CheckIfRewindingPossible();
		if (IsRewinding)
		{
			CachedTransform.position = TransformSystem.CurrentPosition(_myIndex);
			if (updateCount == 0)
			{
				_despawnOnRewindEvent.Apply(isRewind: true);
			}
		}
	}

	private int CheckIfRewindingPossible()
	{
		var updateCount = TransformSystem.UpdateCount(_myIndex);
		HadSomethingToRewindToAtFrameStart = updateCount > 0;
		return updateCount;
	}

	public override void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false)
	{
		if (recordImmediately) { _despawnOnRewindEvent = evt; }
		else { throw new System.Exception("Bullet has no idea what to do with some event."); }
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		_velocityController = velocityController;

		if (_myIndex < 0)
		{
			_myIndex = TransformSystem.GetNewComponent(CachedTransform.position, velocityController.CurrentVelocityUnitsPerFrame);
		}
		else
		{
			TransformSystem.ResetExistingComponent(_myIndex, CachedTransform.position, velocityController.CurrentVelocityUnitsPerFrame);
		}
	}

	public override void Reset()
	{
		//Do I even have to do this?
		/*TransformSystem.ResetExistingComponent(_myIndex, CachedTransform.position, 
														 _velocityController.CurrentVelocityUnitsPerFrame);*/
	}
}