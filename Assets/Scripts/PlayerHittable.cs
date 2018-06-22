using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHittable : AHittable
{
	public ShipController playerShip;

	public float startingHP = 10;

	private float _currentHP;
	public float CurrentHP { get { return _currentHP; } }
	private List<PendingDamage> _pendingDamage = new List<PendingDamage>();

	private void OnTriggerEnter(Collider other)
	{
		if (!playerShip.IsRewinding)
		{
			TryEnqueueHitEvent(other.GetComponent<Damage>());   // -> lot of GetComponents there; lookup by
																//	tag would be better
		}
	}

	public override void Init()
	{
		_currentHP = startingHP;
		_pendingDamage.Clear();
	}

	private void FixedUpdate()
	{
		var dt = playerShip.IsRewinding ? -Time.fixedDeltaTime : Time.fixedDeltaTime;
		ProcessPendingDamage(dt);
	}

	private void ProcessPendingDamage(float deltaTime)
	{
		if (_pendingDamage.Count > 0)
		{
			foreach (var dmg in _pendingDamage)
			{
				var shouldApplyDamage = dmg.UpdateAndCheckIfNeedsToApply(deltaTime);
				if (shouldApplyDamage && !playerShip.IsRewinding)
				{
					playerShip.EnqueueEvent(new HitStunOverEvent(dmg.damage, this));
				}
			}
		}
	}

	public override void ApplyHitStunOver(int dmg, bool isRewind)
	{
		if (!isRewind)
		{
			DoApplyDamage(dmg, isRewind);
		}
		else
		{
			UndoDamageButMakeItPending(dmg);
		}
	}

	public override void Hit(int dmg, bool isBounds, bool isRewind)
	{
		// Not checking isBounds because for now that can't happen
		if (!isRewind) { AddPendingDamage(dmg); }
		else { TryRemoveFromPendingDamage(dmg, isRewind); }
	}

	public override void Stop()
	{
		// TODO - what was this again?
	}

	private void DoApplyDamage(int damage, bool isRewind)
	{
		_currentHP -= damage;
		TryRemoveFromPendingDamage(damage, isRewind);
		if (_currentHP <= 0)
		{
			// Debug.Log("Player is now dead.");
			// TODO - figure out what to do about this,
			//		  without involving a graveyard
		}
	}

	private void AddPendingDamage(int damage)
	{
		_pendingDamage.Add(new PendingDamage(timeLeft: 0.1f, damage: damage));
	}

	private bool TryRemoveFromPendingDamage(int damage, bool isRewind)
	{
		int indexToRemove = -1;
		for (int i = 0; i < _pendingDamage.Count && indexToRemove == -1; ++i)
		{
			if (_pendingDamage[i].damage == damage)
			{
				if ((isRewind && _pendingDamage[i].timeLeft > 0f) ||
					(!isRewind && _pendingDamage[i].timeLeft <= 0f))
				{
					indexToRemove = i;
				}
			}
		}

		if (indexToRemove != -1) { _pendingDamage.RemoveAt(indexToRemove); }

		// If we have no pending damage left, could refresh visuals

		return indexToRemove != -1;
	}

	private void UndoDamageButMakeItPending(int damage)
	{
		_pendingDamage.Add(new PendingDamage(timeLeft: 0f, damage: damage));
		_currentHP += damage;
	}

	private void TryEnqueueHitEvent(Damage dmg)
	{
		if (dmg != null)
		{
			var hitEvent = new HitEvent(dmg.damage, dmg.isBounds, this);
			playerShip.EnqueueEvent(hitEvent);
		}
	}
}
