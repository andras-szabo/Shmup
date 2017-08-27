using System.Collections.Generic;
using System.Linq;

public static class ScriptParser
{
	public static List<ScriptCommand> ParseFile(string fileAsText, ScriptLanguageDefinition languageDef)
	{
		var commands = new List<ScriptCommand>();

		var lines = SplitIntoLines(fileAsText, removeEmptyLines: true);
		foreach (var line in lines)
		{
			commands.Add(ParseLine(line, languageDef));
		}

		return commands;
	}

	public static string[] SplitIntoLines(string fileAsText, bool removeEmptyLines = true)
	{
		var lines = fileAsText.Split('\n');

		if (removeEmptyLines)
		{
			var asList = new List<string>(lines);
			asList.RemoveAll(line => string.IsNullOrEmpty(line));
			lines = asList.ToArray();
		}

		return lines;
	}

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
			command.id = cmdDef.id;
			if (cmdDef.argumentCount > 0)
			{
				command.args = new System.Object[cmdDef.argumentCount];
				for (int i = 0; i < cmdDef.argumentCount; ++i)
				{
					token = queue.Dequeue();
					var argType = cmdDef.argumentTypes[i];
					command.args[i] = System.ComponentModel.TypeDescriptor
										.GetConverter(argType)
										.ConvertFromString(token);
				}
			}
		}

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
