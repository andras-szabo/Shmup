using System.Collections.Generic;
using UnityEngine;

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

	private Vector3 _velocityVPperSecond;
	private Vector3 _velocityWorldUnitsPerFrame;

	protected Queue<IRewindableEvent> _eventQueue = new Queue<IRewindableEvent>();

	public bool IsInGraveyard { get; private set; }
	public bool log;
	public bool IsRewinding { get { return rewindable != null && rewindable.IsRewinding; } }

	private int _lifeSpanClientIndex = -1;

	private bool _alreadySignedUpForOnDespawn;

	//TODO: Major cleanup needed. "InitEverythingElse? ...as in, everything
	//		else than what?! *facepalm*
	public override void Init(string param)
	{
		_velocityVPperSecond = CachedTransform.up * startSpeedViewportPerSecond;
		var worldVelocity = ViewportUtility.ViewportToWorldVelocity(_velocityVPperSecond);
		_velocityWorldUnitsPerFrame = worldVelocity / Consts.IG_FRAMERATE;
		InitEverythingElse();
	}

	public override void Init(Vector2 velocity, Vector3 spin)
	{
		// TODO - fix dangling spin
		this._velocityVPperSecond = velocity;
		_velocityWorldUnitsPerFrame = _velocityVPperSecond / Consts.IG_FRAMERATE;

		InitEverythingElse();
	}

	private void InitEverythingElse()
	{
		InitializeRewindableAndEventQueue();
		InitializeGraveyardStatus();
		InitializeHittable();

		//TODO: warning, watch out with multicast delegate call on each
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
		rewindable.Init(_velocityWorldUnitsPerFrame);
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
		if (myRenderer != null) { myRenderer.enabled = enable; }
		//TODO fix
		if (hittable != null) { hittable.Collider.enabled = false; }
		if (myWeight != null) { myWeight.enabled = enable; }
	}
}
