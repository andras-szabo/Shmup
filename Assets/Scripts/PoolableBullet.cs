using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PoolableBullet : APoolable, IGraveyardCapable
{
	public Renderer myRenderer;
	public Collider myCollider;
	public AHittable hittable;
	public SpaceBendingObject myWeight;
	public ECSBulletRewindable rewindable;

	public bool canRotate;

	public bool hasLimitedLifeSpan;
	public float lifeSpanSeconds = 0.35f;

	public float startSpeedViewportPerSecond;

	protected SpinController _spinController = new SpinController();
	protected VelocityController _velocityController = new VelocityController();

	protected Queue<IRewindableEvent> _eventQueue = new Queue<IRewindableEvent>();

	public bool IsInGraveyard { get; private set; }
	public bool log;
	public bool IsRewinding { get { return rewindable != null && rewindable.IsRewinding; } }

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

		//TODO: warning, watch out with multicast delegate call on each
		OnDespawn += CleanupRewindable;
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
		if (rewindable == null) { return; }
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
		//TODO: so this should be unified: either just do the call
		//		and clients can listen to it, or have the entity
		//		call all the relevant guys to do their shit.
		//		also maybe init could be better via signals.
		EnableVisuals(false);
		IsInGraveyard = true;

		if (rewindable != null)
		{
			rewindable.GoToGraveyard();
		}
	}

	public virtual void GetOutOfGraveyard()
	{
		EnableVisuals(true);
		IsInGraveyard = false;
	}

	private void EnableVisuals(bool enable)
	{
		myRenderer.enabled = enable;
		//TODO fix
		if (hittable != null) { hittable.Collider.enabled = false; }
		if (myWeight != null) { myWeight.enabled = enable; }
	}
}
