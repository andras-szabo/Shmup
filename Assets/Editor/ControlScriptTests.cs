using UnityEngine;
using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

public class ControlScriptTests
{
	[Test]
	public void ReadTestFileTest()
	{
		var path = Path.Combine(Consts.PATH_CONTROL_SCRIPTS, "test.scr");
		var fileAsText = Resources.Load<TextAsset>(path).text;
		var lines = fileAsText.Split('\n');

		var testLanguageDef = CreateTestLanguageDefinition();
		var first = ScriptParser.ParseLine(lines[0], testLanguageDef); 
		Assert.IsTrue(first.args.Length == 4);
		Assert.IsTrue((string)first.args[0] == "a", (string) first.args[0]);
		Assert.IsTrue((string)first.args[1] == "b");
		Assert.IsTrue((string)first.args[2] == "c");
		Assert.IsTrue((int)first.args[3] == 12);
		Assert.IsTrue(Mathf.Approximately(first.delay, 0f));

		var second = ScriptParser.ParseLine(lines[1], testLanguageDef);
		Assert.IsTrue(second.args == null);
		Assert.IsTrue(second.id == 2);
		Assert.IsTrue(Mathf.Approximately(second.delay, 0.2f));

		var third = ScriptParser.ParseLine(lines[2], testLanguageDef);
		Assert.IsTrue(third.args.Length == 2);
		Assert.IsTrue(third.id == 3);
		Assert.IsTrue(Mathf.Approximately((float)third.args[0], 1f));
		Assert.IsTrue(Mathf.Approximately((float)third.args[1], 2.2f));
		Assert.IsTrue(Mathf.Approximately(third.delay, 0.3f));
	}

	private ScriptLanguageDefinition CreateTestLanguageDefinition()
	{
		var def = new ScriptLanguageDefinition();

		var cmd1 = new ScriptCommandDefinition(1, "test", 4, new List<Type> { typeof(string), typeof(string), typeof(string), typeof(int) });
		var cmd2 = new ScriptCommandDefinition(2, "other", 0, null);
		var cmd3 = new ScriptCommandDefinition(3, "third", 2, new List<Type> { typeof(float), typeof(float) });

		def.Add(cmd1, cmd2, cmd3);

		return def;
	}
}
