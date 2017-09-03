using System.IO;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Spawner))]
public class EnemySpawner : MonoWithCachedTransform
{
	public Spawner spawner;

	public void SpawnWithScript(string scriptName)
	{
		spawner.SpawnFromPool(scriptName);
	}

	//TODO: Clearly we should cache commands instead of parsin them
	//		every single time
	public static List<ICommand> LoadScript(string scriptName, ScriptLanguageDefinition scriptDefinition, IParser parser)
	{
		if (string.IsNullOrEmpty(scriptName))
		{
			return null;
		}

		var path = Path.Combine(Consts.PATH_CONTROL_SCRIPTS, string.Format("{0}.scr", scriptName));
		var fileAsText = Resources.Load<TextAsset>(path).text;
		var commands = ScriptParser.ParseFile(fileAsText, scriptDefinition);

		var l = new List<ICommand>(commands.Count);

		foreach (var cmd in commands)
		{
			l.Add(parser.Parse(cmd));
		}

		return l;
	}
}
