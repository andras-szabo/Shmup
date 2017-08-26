using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(ShipScriptRunner))]
public class SetupTestShipScript : MonoBehaviour
{
	void Start()
	{
		var runner = GetComponent<ShipScriptRunner>();
		var cmds = GetTestScript();
		runner.Run(cmds);
	}

	private List<IShipCommand> GetTestScript()
	{
		var path = Path.Combine(Application.dataPath, "ControlScripts/spinTest.scr");
		var lines = File.ReadAllLines(path);
		var languageDef = ShipScriptDefinition.Define();

		var l = new List<IShipCommand>(lines.Length);

		foreach (var line in lines)
		{
			var cmd = ScriptParser.ParseLine(line, languageDef);
			l.Add(ShipCommandFactory.Parse(cmd));
		}

		return l;
	}
}
