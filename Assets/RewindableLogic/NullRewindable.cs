public class NullRewindable : ABaseRewindable
{
	public override void EnqueueEvent(IRewindableEvent evt, bool recordImmediately = false)
	{
	}

	public override void Init(VelocityController velocityController, SpinController spinController)
	{
	}

	public override void Reset()
	{
	}
}