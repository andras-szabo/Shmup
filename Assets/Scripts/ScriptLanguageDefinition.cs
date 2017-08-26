using System;
using System.Collections;
using System.Collections.Generic;

public class ScriptLanguageDefinition : Dictionary<string, ScriptCommandDefinition>
{
	public void Add(params ScriptCommandDefinition[] commands)
	{
		foreach (var command in commands)
		{
			Add(command.token.ToLower(), command);
		}
	}
}

public class ScriptCommandDefinition
{
	public readonly uint id;
	public readonly string token;
	public readonly int argumentCount;
	public readonly List<Type> argumentTypes;

	public ScriptCommandDefinition(uint id, string token, int argumentCount, List<Type> argumentTypes)
	{
		this.id = id;
		this.token = token;
		this.argumentCount = argumentCount;
		this.argumentTypes = argumentTypes;
	}
}