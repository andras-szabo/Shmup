using UnityEngine;
using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ControlScriptTests
{
	[Test]
	public void LoopRegexTest()
	{
		var reg = new Regex(ScriptParser.LoopRegexPattern);
		var match = reg.Match("for i = 1 to 10 step 2");

		Assert.IsTrue(match.Groups["loopVarName"].ToString() == "i");
		Assert.IsTrue(match.Groups["loopStart"].ToString() == "1");
		Assert.IsTrue(match.Groups["loopEnd"].ToString() == "10");
		Assert.IsTrue(match.Groups["loopStep"].ToString() == "2");
		Assert.IsTrue(string.IsNullOrEmpty(match.Groups["delay"].ToString()));

		match = reg.Match("0.2   for loopy=1 to 999");
		Assert.IsTrue(match.Success);
		Assert.IsTrue(match.Groups["delay"].ToString() == "0.2");
		Assert.IsTrue(match.Groups["loopVarName"].ToString() == "loopy");
		Assert.IsTrue(match.Groups["loopStart"].ToString() == "1");
		Assert.IsTrue(match.Groups["loopEnd"].ToString() == "999");

		match = reg.Match("for j = 10 to 1 step -1");
		Assert.IsTrue(match.Success);
		Assert.IsTrue(match.Groups["loopStep"].ToString() == "-1");
	}

	[Test]
	public void DecimalRegexTest()
	{
		var reg = new Regex(ScriptParser.DecimalRegexPattern);

		M(reg, "0");	M(reg, "0.2");	M(reg, "12.345");	M(reg, "0.002");
		M(reg, "123");	M(reg, "-0.1");	M(reg, "-12");		M(reg, "-0.002");

		M(reg, "a", false);
		M(reg, "abcd.efg", false);
		M(reg, "0.asd", false);
		M(reg, "someString.123", false);
	}

	[Test]
	public void VariableDeclarationRegexTest()
	{
		var reg = new Regex(ScriptParser.VariableDeclarationPattern);
		M(reg, "var i = 0");
		M(reg, "var Foo = 12.34");
		M(reg, "var   blah = 1");
		M(reg, "var neg = -1");
		M(reg, "var someCamelCaseVariable = 0.4321");

		M(reg, "var i = able", false);
		M(reg, "var i 12.3", false);
	}

	private void M(Regex regex, string expr, bool shouldMatch = true)
	{
		Assert.IsTrue(regex.IsMatch(expr) == shouldMatch, expr);
	}

	[Test]
	public void ForLoopTest()
	{
		var script = "for i = 1 to 10 step 1\nspawn someType 1 2 someScript\nend";
		var commands = ParserUtility.ParseFile(script, SpawnerScriptDefinition.Define());
		Assert.IsTrue(commands.Count == 10, commands.Count.ToString()); 
	}

	[Test]
	public void FreeVariablesTest()
	{
		var script = "var i = 1\n0.2*i spawn someType 1 i somescript\n";
		var commands = ParserUtility.ParseFile(script, SpawnerScriptDefinition.Define());
		Assert.IsTrue(commands.Count == 1, commands.Count.ToString());
		Assert.IsTrue(Mathf.Approximately(commands[0].delay, 0.2f), commands[0].delay.ToString());
		Assert.IsTrue((float)(commands[0].args[2]) == 1, commands[0].args[2].ToString());
	}

	[Test]
	public void OtherVariableTest()
	{
		var script = "var foo = 0.35\nvar bar = 0.2\nfoo spawn someType 1 bar*10 somescript\n";
		var commands = ParserUtility.ParseFile(script, SpawnerScriptDefinition.Define());
		Assert.IsTrue(commands.Count == 1, commands.Count.ToString());
		Assert.IsTrue(Mathf.Approximately(commands[0].delay, 0.35f), commands[0].delay.ToString());
		Assert.IsTrue(Mathf.Approximately((float)(commands[0].args[2]), 2f), commands[0].args[2].ToString());
	}

	[Test]
	public void LoopInLoopTest()
	{
		var script = "for i = 1 to 3\nfor j = 1 to 3\n0.2*i spawn someType 1 2 someScript\nend\nend";
		var commands = ParserUtility.ParseFile(script, SpawnerScriptDefinition.Define());
		Assert.IsTrue(commands.Count == 9, commands.Count.ToString());
	}

	[Test]
	public void LoopWithVariableTest()
	{
		var script = "for i = 1 to 2\n0.2 spawn someType i*2 i someScript\nend";
		var commands = ParserUtility.ParseFile(script, SpawnerScriptDefinition.Define());
		Assert.IsTrue(commands.Count == 2, commands.Count.ToString());
		Assert.IsTrue((float)(commands[0].args[1]) == 2 && (float)(commands[0].args[2]) == 1);
		Assert.IsTrue((float)(commands[1].args[1]) == 4 && (float)(commands[1].args[2]) == 2);
	}

	[Test]
	public void ReadTestFileTest()
	{
		var path = Path.Combine(Consts.PATH_CONTROL_SCRIPTS, "test.scr");
		var fileAsText = Resources.Load<TextAsset>(path).text;
		var lines = fileAsText.Split('\n');

		var testLanguageDef = CreateTestLanguageDefinition();
		var parser = new ScriptParser(lines, testLanguageDef);

		var first = parser.ParseLine(lines[0]);
		Assert.IsTrue(first.args.Length == 4);
		Assert.IsTrue((string)first.args[0] == "a", (string) first.args[0]);
		Assert.IsTrue((string)first.args[1] == "b");
		Assert.IsTrue((string)first.args[2] == "c");
		Assert.IsTrue((int)first.args[3] == 12);
		Assert.IsTrue(Mathf.Approximately(first.delay, 0f));

		var second = parser.ParseLine(lines[1]);
		Assert.IsTrue(second.args == null);
		Assert.IsTrue(second.id == 2);
		Assert.IsTrue(Mathf.Approximately(second.delay, 0.2f), string.Format("{0}", second.delay));

		var third = parser.ParseLine(lines[2]);
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
