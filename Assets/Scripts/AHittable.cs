using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class AHittable : MonoBehaviour, IHittable
{
	public Collider myCollider;
	public Collider Collider
	{
		get
		{
			return myCollider;
		}
	}

	public abstract void ApplyHitStunOver(int dmg, bool isRewind);
	public abstract void Hit(int dmg, bool isBounds, bool isRewind);
	public abstract void Init();
	public abstract void Stop();
}