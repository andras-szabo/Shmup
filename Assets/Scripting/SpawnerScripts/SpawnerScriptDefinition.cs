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

		def.Add(spawnCommand);
		return def;
	}
}