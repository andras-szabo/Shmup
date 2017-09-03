public class SpawnerCommandFactory : IParser
{
	private static SpawnerCommandFactory _instance = new SpawnerCommandFactory();
	public static SpawnerCommandFactory Instance
	{
		get
		{
			return _instance;
		}
	}

	public ICommand Parse(SerializedScriptCommand cmd)
	{
		switch (cmd.id)
		{
			case 1: return new SpawnerScriptSpawn(cmd);
		}

		return null;
	}
}
