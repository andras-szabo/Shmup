using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(ScriptRunner))]
public class SetupTestShipScript : MonoBehaviour
{
	void Start()
	{
		var runner = GetComponent<ScriptRunner>();
		var cmds = GetTestScript();
		runner.Run(cmds);
	}

	//TODO: Clearly this should not be called for every
	//		single instance
	private List<ICommand> GetTestScript()
	{
		var definition = ShipScriptDefinition.Define();
		var parser = new ShipCommandFactory();
		return EnemySpawner.LoadScript("spinTest.scr", definition, parser);
	}
}
