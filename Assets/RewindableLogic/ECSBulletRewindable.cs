using UnityEngine;

public class ECSBulletRewindable : ABaseRewindable
{
	public const bool USE_JOBS = true;

	private RewindableService _rewindService;
	protected RewindableService RewindService { get { return _rewindService ?? (_rewindService = RewindableService.Instance); } }

	private ITransformSystem _transformSystem;
	private ITransformSystem TRSystem
	{
		get
		{
			if (_transformSystem == null)
			{
				if (USE_JOBS) { _transformSystem = TransformSystemWithJobs.Instance; }
				else { _transformSystem = TransformSystem.Instance;  }
			}

			return _transformSystem;
		}
	}

	public PoolableBullet myBullet;

	private IRewindableEvent _despawnOnRewindEvent;
	private int _myIndex = -1;

	public Vector3 Position
	{
		get { return CachedTransform.position; }
		set { CachedTransform.position = value; }
	}

	public void GoToGraveyard()
	{
		TRSystem.GoToGraveyard(_myIndex);
	}

	/*
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
	*/

	public void SetIsRewind(bool rewind, bool hadSomething)
	{
		IsRewinding = rewind;
		HadSomethingToRewindToAtFrameStart = hadSomething;
	}

	public void SetHadSomethingToRewind(bool state)
	{
		HadSomethingToRewindToAtFrameStart = state;
	}

	public void CallDespawnOnRewind()
	{
		_despawnOnRewindEvent.Apply(isRewind: true);
	}

	public void JustCallDespawn()
	{
		myBullet.Despawn(false);
	}

	public void GetOutOfGraveyard()
	{
		myBullet.GetOutOfGraveyard();
	}

	/*private int CheckIfRewindingPossible()
	{
		var updateCount = TRSystem.UpdateCount(_myIndex);
		HadSomethingToRewindToAtFrameStart = updateCount > 0;
		return updateCount;
	}*/

	public override void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false)
	{
		if (recordImmediately) { _despawnOnRewindEvent = evt; }
		else { throw new System.Exception("Bullet has no idea what to do with some event."); }
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
		throw new System.NotImplementedException();
	}

	public void Init(Vector3 velocityPerFrame)
	{
		if (_myIndex < 0)
		{
			_myIndex = TRSystem.GetNewComponent(this, velocityPerFrame);
		}
		else
		{
			TRSystem.ResetExistingComponent(_myIndex, CachedTransform.position, velocityPerFrame);
		}
	}

	public override void Reset()
	{
		TRSystem.SetStatusToDespawned(_myIndex);
	}
}