using System.Collections.Generic;
using System.Linq;

public static class ParserUtility
{
	public static List<SerializedScriptCommand> ParseFile(string fileAsText, ScriptLanguageDefinition languageDef)
	{
		var lines = SplitIntoLines(fileAsText, removeEmptyLines: true);
		return new ScriptParser(lines, languageDef).Parse();
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