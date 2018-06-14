public class PendingDamage
{
	public PendingDamage(float timeLeft, int damage)
	{
		this.timeLeft = timeLeft;
		this.damage = damage;
	}

	public float timeLeft;
	public int damage;

	public bool UpdateAndCheckIfNeedsToApply(float deltaTime)
	{
		timeLeft -= deltaTime;
		return timeLeft <= 0f && (timeLeft + deltaTime > 0f);
	}
}
