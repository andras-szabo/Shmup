using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShipCommandFactory
{
	public static IShipCommand Parse(ScriptCommand cmd)
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
