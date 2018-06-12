using UnityEngine;

public abstract class ACommand : ICommand
{
	public static float CommandUpdateInterval = 0.1f;
	public static WaitForSeconds CommandUpdateIntervalObject = new WaitForSeconds(CommandUpdateInterval);

	public float Delay { get { return delay; } }
	public virtual bool IsControlFlow { get { return false; } }

	public ACommand(SerializedScriptCommand cmd)
	{
		this.delay = cmd.delay;
		this.commandID = cmd.id;
	}

	public readonly float delay;
	public readonly uint commandID;

	public abstract void Execute(IExecutionContext context);
}