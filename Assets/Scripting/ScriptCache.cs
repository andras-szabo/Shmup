using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCache
{
	private Dictionary<string, List<ICommand>> _cachedScripts = new Dictionary<string, List<ICommand>>();

	private static ScriptCache _instance;
	private static ScriptCache Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new ScriptCache();
			}

			return _instance;
		}
	}

	public static void Cleanup()
	{
		if (_instance != null)
		{
			_instance.Clear();
		}
		_instance = null;
	}

	public static List<ICommand> LoadScript(string scriptName, ScriptLanguageDefinition scriptDefinition, IParser parser)
	{
		if (string.IsNullOrEmpty(scriptName)) { return null; }

		List<ICommand> l;
		if (Instance.TryGet(scriptName, out l))
		{
			return l;
		}

		var path = Path.Combine(Consts.PATH_CONTROL_SCRIPTS, string.Format("{0}.scr", scriptName));
		var fileAsText = Resources.Load<TextAsset>(path).text;
		var commands = ParserUtility.ParseFile(fileAsText, scriptDefinition);

		foreach (var cmd in commands)
		{
			l.Add(parser.Parse(cmd));
		}

		Instance.Add(scriptName, l);

		return l;
	}

	private bool TryGet(string scriptName, out List<ICommand> l)
	{
		if (_cachedScripts.TryGetValue(scriptName, out l))
		{
			return true;
		}

		l = new List<ICommand>();
		return false;
	}

	private void Add(string scriptName, List<ICommand> l)
	{
		_cachedScripts.Add(scriptName, l);
	}

	private void Clear()
	{
		_cachedScripts.Clear();
	}
}