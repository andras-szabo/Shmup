using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PoolableEntity : APoolable, IGraveyardCapable
{
	public Renderer myRenderer;
	public Collider myCollider;
	public AHittable hittable;
	public SpaceBendingObject myWeight;
	public ABaseRewindable rewindable;

	public bool canRotate;

	public bool hasLimitedLifeSpan;
	public float lifeSpanSeconds = 0.35f;

	public float startSpeedViewportPerSecond;

	protected SpinController _spinController = new SpinController();
	protected VelocityController _velocityController = new VelocityController();

	protected int _framesSpentInGraveyard;
	protected Queue<IRewindableEvent> _eventQueue = new Queue<IRewindableEvent>();

	public bool log;
	public bool IsInGraveyard { get; protected set; }
	public bool IsRewinding { get { return rewindable.IsRewinding; } }

	private bool _alreadySignedUpForOnDespawn;
	private int _lifeSpanClientIndex = -1;

	//TODO: Major cleanup needed. "InitEverythingElse? ...as in, everything
	//		else than what?! *facepalm*
	public override void Init(string param)
	{
		var startVelocity = CachedTransform.up * startSpeedViewportPerSecond;
		InitializeTransformControllers(startVelocity, Vector3.zero);
		InitEverythingElse();
	}

	public override void Init(Vector2 velocity, Vector3 spin)
	{
		InitializeTransformControllers(velocity, spin);
		InitEverythingElse();
	}

	private void InitEverythingElse()
	{
		InitializeRewindableAndEventQueue();
		InitializeGraveyardStatus();
		InitializeHittable();

		if (!_alreadySignedUpForOnDespawn)
		{
			OnDespawn += CleanupRewindable;
			_alreadySignedUpForOnDespawn = true;
			_dontRemoveOnDespawnListener = true;
		}
	}

	private void CleanupRewindable(bool despawnBecauseRewind)
	{
		rewindable.Reset();
	}

	public override void Stop()
	{
		if (hittable != null)
		{
			hittable.Stop();
		}
	}

	private void InitializeTransformControllers(Vector2 velocity, Vector3 spin)
	{
		_spinController.Reset();
		_velocityController.Reset();

		_velocityController.AccelerateTo(velocity, 0f, 0f);
		_spinController.SpinTo(spin, 0f, 0f);
	}

	private void InitializeHittable()
	{
		if (hittable != null)
		{
			hittable.Init();
		}
	}

	private void InitializeGraveyardStatus()
	{
		GetOutOfGraveyard();
		_framesSpentInGraveyard = 0;

		if (hasLimitedLifeSpan)
		{
			if (_lifeSpanClientIndex < 0)
			{
				_lifeSpanClientIndex = LifeSpanSystem.Instance.RegisterAsNewClient(this, lifeSpanSeconds);
			}
			else
			{
				LifeSpanSystem.Instance.RegisterAsExistingClient(_lifeSpanClientIndex, lifeSpanSeconds);
			}
		}
	}

	private void InitializeRewindableAndEventQueue()
	{
		rewindable.Init(_velocityController, _spinController);
		rewindable.EnqueueEvent(new DespawnOnReplayEvent(this), recordImmediately: true);
		_eventQueue.Clear();
	}

	public void EnqueueEvent(IRewindableEvent evt)
	{
		_eventQueue.Enqueue(evt);
		rewindable.EnqueueEvent(evt);
	}

	public virtual void GoToGraveyard()
	{
		IsInGraveyard = true;
		_framesSpentInGraveyard = 0;

		//TODO: so this should be unified: either just do the call
		//		and clients can listen to it, or have the entity
		//		call all the relevant guys to do their shit.
		//		also maybe init could be better via signals.
		EnableVisuals(false);
		rewindable.Paused = true;
	}

	public virtual void GetOutOfGraveyard()
	{
		IsInGraveyard = false;
		EnableVisuals(true);
		rewindable.Paused = false;
	}

	protected virtual void FixedUpdate()
	{
		if (!rewindable.IsRewinding && !IsInGraveyard)
		{
			UpdateTransform();
			ProcessEventQueue();
		}

		UpdateGraveyardStatus();
	}

	private void UpdateTransform()
	{
		if (canRotate)
		{
			CachedTransform.Rotate(_spinController.RotationPerFrame);
		}

		CachedTransform.position += _velocityController.CurrentVelocityUnitsPerFrame;
	}

	private void UpdateGraveyardStatus()
	{
		if (IsInGraveyard)
		{
			if (!rewindable.IsRewinding && ++_framesSpentInGraveyard == Rewindable.LOG_SIZE_FRAMES)
			{
				Despawn(despawnBecauseRewind: false);
			}

			if (rewindable.IsRewinding
				&& rewindable.HadSomethingToRewindToAtFrameStart
				&& --_framesSpentInGraveyard == 0)
			{
				GetOutOfGraveyard();
			}
		}
	}

	private void ProcessEventQueue()
	{
		if (_eventQueue.Count > 0)
		{
			foreach (var evt in _eventQueue)
			{
				evt.Apply(false);
			}

			_eventQueue.Clear();
		}
	}

	private void EnableVisuals(bool enable)
	{
		myRenderer.enabled = enable;
		if (hittable != null) { hittable.Collider.enabled = enable; }
		if (myWeight != null) { myWeight.enabled = enable; }
	}
}
