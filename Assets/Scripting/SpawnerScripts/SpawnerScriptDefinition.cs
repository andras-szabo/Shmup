﻿using System;
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

		var bgRotCommand = new ScriptCommandDefinition(3, "bgRot", 2, new List<Type>
		{
			typeof(float),		// desired rotation angle in degrees
			typeof(float)		// duration of the rotation
		});

		var spawnWithVelocityAndSpinCommand = new ScriptCommandDefinition(4, "spawnAndStart", 8, new List<Type>
		{
			typeof(string),		// what
			typeof(float),		// where.x
			typeof(float),		// where.y
			typeof(float),		// vel.x
			typeof(float),		// vel.y
			typeof(float),		// spin.x
			typeof(float),		// spin.y
			typeof(float),		// spin.z
		});

		def.Add(spawnCommand);
		def.Add(bgScrollCommand);
		def.Add(bgRotCommand);
		def.Add(spawnWithVelocityAndSpinCommand);

		return def;
	}
}