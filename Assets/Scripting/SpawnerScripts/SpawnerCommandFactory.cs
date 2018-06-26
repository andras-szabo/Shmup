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
			case 2: return new SpawnerScriptBgVel(cmd);
			case 3: return new SpawnerScriptBgRot(cmd);
			case 4: return new SpawnerScriptSpawnWithVelocityAndSpin(cmd);
		}

		return null;
	}
}
