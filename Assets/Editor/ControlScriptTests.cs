using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

public class ControlScriptTests
{
	[Test]
	public void ReadTestFileTest()
	{
		var path = Path.Combine(Application.dataPath, "ControlScripts/test.scr");
		var lines = File.ReadAllLines(path);
		Assert.IsTrue(lines.Length == 3);

		var first = ScriptParser.ParseLine(lines[0], CreateTestLanguageDefinition());
	}

	private ScriptLanguageDefinition CreateTestLanguageDefinition()
	{
		var def = new ScriptLanguageDefinition();

		var cmd1 = new ScriptCommandDefinition("test", 4, new List<Type> { typeof(string), typeof(string), typeof(string), typeof(int) });
		var cmd2 = new ScriptCommandDefinition("other", 0, null);
		var cmd3 = new ScriptCommandDefinition("third", 2, new List<Type> { typeof(float), typeof(float) });

		def.Add(cmd1, cmd2, cmd3);

		return def;
	}
}
