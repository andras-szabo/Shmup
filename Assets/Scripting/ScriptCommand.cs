// This is not the command, this is just the serialized
// version, which should be named appropriately
// - this is there so we can do generic parsing of commands.
public struct ScriptCommand
{
	public float delay;
	public uint id;
	public object[] args;
}
