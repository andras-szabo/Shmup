using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(PoolableEntity))]
public class Hittable : AHittable
{
	public PoolableEntity myEntity;

	public float startingHP = 10;

	protected float _currentHP;
	public float CurrentHP { get { return _currentHP; } }

	public Material normalMaterial;
	public Material hitMaterial;

	private bool _isHit;
	private const float _visualHitStunSeconds = 0.1f;

	private List<PendingDamage> _pendingDamage = new List<PendingDamage>();

	public List<string> PendingDamageInfo()
	{
		var info = new List<string>();
		foreach (var pendingDamage in _pendingDamage)
		{
			info.Add(string.Format("Time left: {0:F3}, dmg: {1}", pendingDamage.timeLeft, pendingDamage.damage));
		}
		return info;
	}

	public override void Init()
	{
		_currentHP = startingHP;
		RefreshVisuals(hit: false);
		_pendingDamage.Clear();
	}

	public override void ApplyHitStunOver(int damage, bool isRewind)
	{
		if (!isRewind)
		{
			DoApplyDamage(damage, isRewind);
		}
		else
		{
			UndoDamageButMakeItPending(damage);
		}
	}

	//TODO: rename to ApplyHit -> same as above
	//			=> but actually maybe with better names,
	//			that already suggest how getting hit works
	//			e.g. "HitStartEvent" and "HitFinishEvent"
	public override void Hit(int damage, bool wasHitByOutOfBoundsBarrier, bool isRewind)
	{
		if (wasHitByOutOfBoundsBarrier)
		{
			MoveImmediatelyToOrFromGraveyard(damage, isRewind);
		}
		else
		{
			if (!isRewind)
			{
				AddPendingDamage(damage);
			}
			else
			{
				TryRemoveFromPendingDamage(damage, isRewind);
			}
		}
	}

	public override void Stop()
	{
		RefreshVisuals(false);
	}

	private void AddPendingDamage(int damage)
	{
		if (!myEntity.IsInGraveyard)
		{
			_pendingDamage.Add(new PendingDamage(timeLeft: _visualHitStunSeconds, damage: damage));
			RefreshVisuals(hit: true);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		//TODO: there must be a better way to do this,
		// without GetComponent; via UID...?!
		if (!myEntity.IsRewinding && !myEntity.IsInGraveyard)
		{
			TryEnqueueHitEvent(other.GetComponent<Damage>());
		}
	}

	private void FixedUpdate()
	{
		if (!myEntity.IsInGraveyard)
		{
			var deltaT = myEntity.IsRewinding ? -Time.fixedDeltaTime : Time.fixedDeltaTime;
			ProcessPendingDamage(deltaT);
		}
	}

	private void ProcessPendingDamage(float deltaTime)
	{
		if (_pendingDamage.Count > 0)
		{
			foreach (var dmg in _pendingDamage)
			{
				var shouldApplyDamage = dmg.UpdateAndCheckIfNeedsToApply(deltaTime);
				if (shouldApplyDamage && !myEntity.IsRewinding)
				{
					myEntity.EnqueueEvent(new HitStunOverEvent(dmg.damage, this));
				}
			}
		}
	}

	private void DoApplyDamage(int damage, bool isRewind)
	{
		_currentHP -= damage;
		TryRemoveFromPendingDamage(damage, isRewind);
		if (_currentHP <= 0 && !myEntity.IsInGraveyard)
		{
			ParticleService.Instance.SpawnParticles(pType: 0, pos: CachedTransform.position);
			myEntity.GoToGraveyard();
		}
	}

	private void UndoDamageButMakeItPending(int damage)
	{
		_pendingDamage.Add(new PendingDamage(timeLeft: 0f, damage: damage));
		_currentHP += damage;
		RefreshVisuals(hit: true);
		if (_currentHP > 0 && myEntity.IsInGraveyard) { myEntity.GetOutOfGraveyard(); }
	}

	private void TryEnqueueHitEvent(Damage dmg)
	{
		if (dmg == null)
		{
			return;
		}

		var hitEvent = new HitEvent(dmg.damage, dmg.isBounds, this);
		myEntity.EnqueueEvent(hitEvent);
	}

	private void MoveImmediatelyToOrFromGraveyard(int damage, bool isRewind)
	{
		_currentHP -= (isRewind ? (damage * -1) : damage);

		if (_currentHP < 0 && !myEntity.IsInGraveyard)
		{
			myEntity.GoToGraveyard();
		}
		else if (_currentHP > 0 && myEntity.IsInGraveyard)
		{
			myEntity.GetOutOfGraveyard();
		}
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

		if (indexToRemove != -1)
		{
			_pendingDamage.RemoveAt(indexToRemove);
		}

		if (_pendingDamage.Count < 1)
		{
			RefreshVisuals(hit: false);
		}

		return indexToRemove != -1;
	}

	private void RefreshVisuals(bool hit)
	{
		if (_isHit != hit)
		{
			_isHit = hit;
			myEntity.myRenderer.material = hit ? hitMaterial : normalMaterial;
		}
	}
}