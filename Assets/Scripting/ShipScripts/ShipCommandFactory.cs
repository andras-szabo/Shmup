public interface IParser
{
	ICommand Parse(SerializedScriptCommand cmd);
}

public class ShipCommandFactory : IParser
{
	private static ShipCommandFactory _instance = new ShipCommandFactory();
	public static ShipCommandFactory Instance { get { return _instance; } }

	public ICommand Parse(SerializedScriptCommand cmd)
	{
		switch (cmd.id)
		{
			case 1: return new ShipScriptRepeat(cmd);
			case 2: return new ShipScriptSpin(cmd);
			case 3: return new ShipScriptEnd(cmd);
			case 4: return new ShipScriptVelocity(cmd);
		}

		return null;
	}
}
