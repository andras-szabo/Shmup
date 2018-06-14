using System.Collections.Generic;
using UnityEngine;

// TODO:
// so then next step maybe is: Enemy : APoolableEntity,
//			=> but such that Enemy has a script
//			=> and Bullet: APoolableEntity, but it adds lifeSpan
//				=> these specialised versions of APoolableEntity will know what to do in their init and stop
//				calls

[RequireComponent(typeof(Renderer))]
public abstract class APoolableEntity : APoolable
{
	public Renderer myRenderer;
	public IHittable hittable;
	public SpaceBendingObject myWeight;
	public IRewindable rewindable;

	protected int _framesSpentInGraveyard;
	protected Queue<IRewindableEvent> _eventQueue = new Queue<IRewindableEvent>();

	public bool IsInGraveyard { get; protected set; }
	public bool IsRewinding { get { return rewindable.IsRewinding; } }

	public void EnqueueEvent(IRewindableEvent evt)
	{
		_eventQueue.Enqueue(evt);
		rewindable.EnqueueEvent(evt);
	}

	public virtual void GoToGraveyard()
	{
		IsInGraveyard = true;
		_framesSpentInGraveyard = 0;
		EnableVisuals(false);
	}

	public virtual void GetOutOfGraveyard()
	{
		IsInGraveyard = false;
		EnableVisuals(true);
	}

	protected virtual void FixedUpdate()
	{
		if (!rewindable.IsRewinding && !IsInGraveyard)
		{
			ProcessEventQueue();
		}

		UpdateGraveyardStatus();
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
		hittable.Collider.enabled = enable;
		if (myWeight != null) { myWeight.enabled = enable; }
	}
}
