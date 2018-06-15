using UnityEngine;

[RequireComponent(typeof(PoolableEntity))]
public class BulletHittable : AHittable
{
	public PoolableEntity myEntity;

	private void OnTriggerEnter(Collider other)
	{
		if (!myEntity.IsRewinding && !myEntity.IsInGraveyard)
		{
			TryHandleHit();
		}
	}

	private void TryHandleHit()
	{
		myEntity.GoToGraveyard();
	}

	public override void ApplyHitStunOver(int dmg, bool isRewind)
	{
		// nope
	}

	public override void Hit(int dmg, bool isBounds, bool isRewind)
	{
		// nope
	}

	public override void Init()
	{
		// nope
	}

	public override void Stop()
	{
		// nope
	}
}
