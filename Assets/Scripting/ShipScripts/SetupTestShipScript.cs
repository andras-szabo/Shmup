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

	//TODO: Clearly this should not be called for every
	//		single instance
	private List<IShipCommand> GetTestScript()
	{
		var path = Path.Combine(Consts.PATH_CONTROL_SCRIPTS, "spinTest.scr");
		var fileAsText = Resources.Load<TextAsset>(path).text;
		var commands = ScriptParser.ParseFile(fileAsText, ShipScriptDefinition.Define());

		var l = new List<IShipCommand>(commands.Count);

		foreach (var cmd in commands)
		{
			l.Add(ShipCommandFactory.Parse(cmd));
		}

		return l;
	}
}
