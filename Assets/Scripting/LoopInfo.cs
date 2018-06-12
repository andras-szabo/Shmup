public class LoopInfo
{
	public LoopInfo(int completionCount, int commandsWithinLoop)
	{
		this.ranToCompletionCount = completionCount;
		this.commandsWithinLoop = commandsWithinLoop;
	}

	public bool IsCountingCommands
	{
		get
		{
			return ranToCompletionCount < 1 && !alreadyCountedCommandsWithinLoop;
		}
	}

	public void Completed()
	{
		alreadyCountedCommandsWithinLoop = true;
		ranToCompletionCount += 1;
	}

	public int GetLoopCommandOffset()
	{
		return ranToCompletionCount * commandsWithinLoop;
	}

	//TODO: hide these and use accessors if have to
	public int ranToCompletionCount;
	public int commandsWithinLoop;
	public bool alreadyCountedCommandsWithinLoop;
}
