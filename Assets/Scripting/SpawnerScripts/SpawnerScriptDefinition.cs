using System;
using System.Collections.Generic;

public static class SpawnerScriptDefinition
{
	public static ScriptLanguageDefinition Define()
	{
		var def = new ScriptLanguageDefinition();

		var spawnCommand = new ScriptCommandDefinition(1, "spawn", 4, new List<Type>
		{
			typeof(string),		// what
			typeof(float),		// where: x
			typeof(float),		// where: y
			typeof(string)		// which script should it run
		});

		var bgScrollCommand = new ScriptCommandDefinition(2, "bgVel", 3, new List<Type>
		{
			typeof(float),		// x
			typeof(float),		// y
			typeof(float)		// acceleration delta
		});

		def.Add(spawnCommand);
		def.Add(bgScrollCommand);

		return def;
	}
}