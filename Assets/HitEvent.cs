﻿public class HitEvent : IRewindableEvent
{
	readonly public int dmg;
	readonly public bool isBounds;
	readonly public IHittable target;

	public HitEvent(int dmg, bool isBounds, IHittable target)
	{
		this.dmg = dmg;
		this.isBounds = isBounds;
		this.target = target;
	}

	public void Apply(bool isRewind)
	{
		target.Hit(dmg, isBounds, isRewind);	
	}
}

public class HitStunOverEvent : IRewindableEvent
{
	readonly public IHittable target;
	readonly public int dmg;

	public HitStunOverEvent(int dmg, IHittable target)
	{
		this.target = target;
		this.dmg = dmg;
	}

	public void Apply(bool isRewind)
	{
		target.ApplyHitStunOver(dmg, isRewind);
	}
}

public interface IHittable
{
	void Hit(int dmg, bool isBounds, bool isRewind);
	void ApplyHitStunOver(int dmg, bool isRewind);
}