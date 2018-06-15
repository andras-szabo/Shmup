using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PoolableEntity : APoolable
{
	public Renderer myRenderer;
	public Collider myCollider;
	public AHittable hittable;
	public SpaceBendingObject myWeight;
	public ABaseRewindable rewindable;

	public bool hasLimitedLifeSpan;
	public float lifeSpanSeconds = 0.35f;
	private float _elapsedLifeSpanSeconds = 0f;

	public float startSpeedViewportPerSecond;

	protected SpinController _spinController = new SpinController();
	protected VelocityController _velocityController = new VelocityController();

	protected int _framesSpentInGraveyard;
	protected Queue<IRewindableEvent> _eventQueue = new Queue<IRewindableEvent>();

	public bool log;
	public bool IsInGraveyard { get; protected set; }
	public bool IsRewinding { get { return rewindable.IsRewinding; } }

	public override void Init(string param)
	{
		InitializeRewindableAndEventQueue();
		InitializeGraveyardStatus();
		InitializeHittable();
		InitializeTransformControllers(startSpeedViewportPerSecond);
	}

	public override void Stop()
	{
		if (hittable != null)
		{
			hittable.Stop();
		}
	}

	private void InitializeTransformControllers(float startSpeed)
	{
		_spinController.Reset();
		_velocityController.Reset();

		if (startSpeed > 0f)
		{
			_velocityController.AccelerateTo(CachedTransform.up * startSpeed, 0f, 0f);
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
		_framesSpentInGraveyard = 0;
		_elapsedLifeSpanSeconds = 0f;
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
	}

	public virtual void GetOutOfGraveyard()
	{
		IsInGraveyard = false;
		EnableVisuals(true);
	}

	protected virtual void FixedUpdate()
	{
		UpdateLifeSpan();
		if (!rewindable.IsRewinding && !IsInGraveyard)
		{
			UpdateTransform();
			ProcessEventQueue();
		}

		UpdateGraveyardStatus();
	}

	private void UpdateTransform()
	{
		CachedTransform.Rotate(_spinController.RotationPerFrame);
		CachedTransform.position += _velocityController.CurrentVelocityUnitsPerFrame;
	}

	private void UpdateLifeSpan()
	{
		if (!IsInGraveyard && hasLimitedLifeSpan)
		{
			if (!IsRewinding)
			{
				_elapsedLifeSpanSeconds += Time.fixedDeltaTime;
				if (_elapsedLifeSpanSeconds >= lifeSpanSeconds)
				{
					GoToGraveyard();
				}
			}
			else
			{
				_elapsedLifeSpanSeconds -= Time.fixedDeltaTime;
			}
		}
	}

	private void UpdateGraveyardStatus()
	{
		if (IsInGraveyard)
		{
			if (!rewindable.IsRewinding && ++_framesSpentInGraveyard == Rewindable.LOG_SIZE_FRAMES)
			{
				Despawn(despawnBecauseRewind: false);
			}

			if (rewindable.IsRewinding && --_framesSpentInGraveyard == 0)
			{
				GetOutOfGraveyard();
			}
		}
	}

	private void ProcessEventQueue()
	{
		foreach (var evt in _eventQueue)
		{
			evt.Apply(false);
		}

		_eventQueue.Clear();
	}

	private void EnableVisuals(bool enable)
	{
		myRenderer.enabled = enable;
		if (hittable != null) { hittable.Collider.enabled = enable; }
		if (myWeight != null) { myWeight.enabled = enable; }
	}
}
