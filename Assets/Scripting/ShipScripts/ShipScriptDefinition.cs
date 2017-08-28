using System;
using System.Collections.Generic;

public static class ShipScriptDefinition
{
	public static ScriptLanguageDefinition Define()
	{
		var def = new ScriptLanguageDefinition();

		var repeat = new ScriptCommandDefinition(1, "repeat", 0, null);
		var spin = new ScriptCommandDefinition(2, "spin", 4, new List<Type>
		{
			typeof(float),
			typeof(float),
			typeof(float),
			typeof(float)
		});
		var end = new ScriptCommandDefinition(3, "end", 0, null);

		// vel x y dt
		var vel = new ScriptCommandDefinition(4, "vel", 3, new List<Type>
		{
			typeof(float),
			typeof(float),
			typeof(float)
		});

		def.Add(repeat, spin, end, vel);

		return def;
	}
}