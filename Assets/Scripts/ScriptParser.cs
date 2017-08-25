using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ScriptParser
{
	public static ScriptCommand ParseLine(string line, ScriptLanguageDefinition languageDef)
	{
		var command = new ScriptCommand();

		var tokens = SplitAndIgnoreWhiteSpaces(line);
		var queue = new Queue<string>(tokens);

		// Get delay if posibol
		var token = queue.Peek();

		try { command.delay = System.Convert.ToSingle(token); queue.Dequeue(); }
		catch (System.FormatException) { }

		token = queue.Dequeue();
		ScriptCommandDefinition cmdDef;
		if (!languageDef.TryGetValue(token, out cmdDef))
		{
			// Syntax error, unrecognized token
		}
		else
		{
			UnityEngine.Debug.Log("yay big success");
		}

		//To be continued

		return command;
	}

	public static string[] SplitAndIgnoreWhiteSpaces(string line)
	{
		var asList = line.Trim().Split().ToList();
		asList.RemoveAll(token => IsAllWhiteSpace(token));
		return asList
				.Select(token => token.ToLower())
				.ToArray();
	}

	public static bool IsAllWhiteSpace(string token)
	{
		if (!string.IsNullOrEmpty(token))
		{
			foreach (var c in token)
			{
				if (!System.Char.IsWhiteSpace(c))
				{
					return false;
				}
			}
		}

		return true;
	}
}
