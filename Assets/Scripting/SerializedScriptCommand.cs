public struct SerializedScriptCommand
{
	public const uint INVALID = 0;

	public static SerializedScriptCommand Invalid()
	{
		var cmd = new SerializedScriptCommand();
		cmd.id = INVALID;
		return cmd;
	}

	public bool IsValid { get { return id != INVALID; } } 

	public float delay;
	public uint id;
	public object[] args;
}
