using System;
using System.Collections.Generic;

public static class ShipScriptDefinition
{
	private static ScriptLanguageDefinition _cachedDefinition;

	public static ScriptLanguageDefinition Define()
	{
		if (_cachedDefinition == null)
		{
			_cachedDefinition = DoDefine();
		}

		return _cachedDefinition;
	}

	private static ScriptLanguageDefinition DoDefine()
	{
		var def = new ScriptLanguageDefinition();

		var repeat = new ScriptCommandDefinition(1, "repeat", 0, null);

		// spin x y w dt -> Set spin speed to Vector3(x, y, z) in dt time (angles / second)
		var spin = new ScriptCommandDefinition(2, "spin", 4, new List<Type>
		{
			typeof(float), typeof(float), typeof(float), typeof(float)
		});
		var end = new ScriptCommandDefinition(3, "endrepeat", 0, null);

		// vel x y dt -> Accelerate to Vector2(x, y) in dt time
		var vel = new ScriptCommandDefinition(4, "vel", 3, new List<Type>
		{
			typeof(float), typeof(float), typeof(float)
		});

		var shoot = new ScriptCommandDefinition(5, "shoot", 0, null);

		// rot x y z -> Set current rotation (w/ Euler angles), effective immediately;
		// so that we can position freshly spawned dudes
		var rot = new ScriptCommandDefinition(6, "rot", 3, new List<Type>
		{
			typeof(float), typeof(float), typeof(float)
		});

		def.Add(repeat, spin, end, vel, shoot, rot);

		return def;
	}
}