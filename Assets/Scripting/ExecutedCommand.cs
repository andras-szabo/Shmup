public struct ExecutedCommand
{
	public ExecutedCommand(float triggerTime, int commandPointer)
	{
		this.triggerTime = triggerTime;
		this.commandPointer = commandPointer;
	}

	public readonly float triggerTime;
	public readonly int commandPointer;
}

