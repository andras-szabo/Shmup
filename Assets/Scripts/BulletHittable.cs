using UnityEngine;

[RequireComponent(typeof(PoolableBullet))]
public class BulletHittable : AHittable
{
	public PoolableBullet myBullet;

	private void OnTriggerEnter(Collider other)
	{
		if (!myBullet.IsRewinding && !myBullet.IsInGraveyard)
		{
			TryHandleHit();
		}
	}

	private void TryHandleHit()
	{
		myBullet.GoToGraveyard();
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
