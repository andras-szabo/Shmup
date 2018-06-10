using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ScriptParser
{
	public static string DecimalRegexPattern = @"^-?\d+(\.\d+)?\z";
	public static string VariableDeclarationPattern = @"^var\s+(?<variableName>\w+)\s*=\s*(?<initialValue>-?\d+(\.\d+)?)\z";
	public static string LoopRegexPattern =
		@"^(?<delay>\d+(\.\d+)?)?\s*for\s+(?<loopVarName>\w+)\s*=\s*(?<loopStart>\d+)\s+to\s+(?<loopEnd>\d+)(\s+step\s+(?<loopStep>-?\d+))?\z";

	private struct LoopWidget
	{
		public string loopVariable;
		public float limit;
		public int lineIndex;
		public float step;

		public bool ShouldContinue(float variableValue)
		{
			return step > 0f ? variableValue <= limit : variableValue >= limit;
		}
	}

	private string[] _lines;
	private ScriptLanguageDefinition _def;

	private Dictionary<string, float> _variables = new Dictionary<string, float>();
	private int _lineIndex;
	private Stack<LoopWidget> _loopStack = new Stack<LoopWidget>();
	private float _loopDelay;

	public ScriptParser(string[] lines, ScriptLanguageDefinition languageDef)
	{
		_lines = lines;
		_def = languageDef;
	}

	public List<SerializedScriptCommand> Parse()
	{
		var commands = new List<SerializedScriptCommand>();

		_lineIndex = 0;

		while (_lineIndex < _lines.Length)
		{
			var command = ParseLine(_lines[_lineIndex]);
			if (command.IsValid)
			{
				commands.Add(command);
				_loopDelay = 0f;
			}
			_lineIndex++;
		}

		return commands;
	}

	public SerializedScriptCommand ParseLine(string line)
	{
		var tokens = ParserUtility.SplitAndIgnoreWhiteSpaces(line);
		var queue = new Queue<string>(tokens);
		var delay = 0f;

		// Get delay if posibol
		var token = queue.Peek();

		try { delay = EE.Evaluate(token, _variables); queue.Dequeue(); }
		catch (System.Collections.Generic.KeyNotFoundException) { }

		if (delay < 0f)
		{
			throw new System.ArgumentException(string.Format("Trying to set negative delay: {0}", line));
		}

		token = queue.Dequeue();
		ScriptCommandDefinition cmdDef;
		if (!_def.TryGetValue(token, out cmdDef))
		{
			if (token == "for") { ParseLoopBegin(line); }
			if (token == "end") { ParseLoopEnd(); }
			if (token == "var") { ParseVariableDeclaration(line); }

			return SerializedScriptCommand.Invalid();
		}

		return ParseCommand(delay, cmdDef, queue);
	}

	private SerializedScriptCommand ParseCommand(float delay, ScriptCommandDefinition cmdDef, Queue<string> queue)
	{
		var command = new SerializedScriptCommand();
		command.id = cmdDef.id;
		command.delay = delay + _loopDelay;
		if (cmdDef.argumentCount > 0)
		{
			command.args = new System.Object[cmdDef.argumentCount];
			for (int i = 0; i < cmdDef.argumentCount; ++i)
			{
				if (queue.Count > 0)
				{
					var token = queue.Dequeue();
					var argType = cmdDef.argumentTypes[i];

					if (argType == typeof(float))
					{
						command.args[i] = EE.Evaluate(token, _variables);
					}
					else
					{
						command.args[i] = System.ComponentModel.TypeDescriptor
									.GetConverter(argType)
									.ConvertFromString(token);
					}
				}
				else
				{
					command.args[i] = null;
				}
			}
		}
		return command;
	}

	private void ParseLoopEnd()
	{
		var loopWidget = _loopStack.Peek();
		_variables[loopWidget.loopVariable] += loopWidget.step;
		if (loopWidget.ShouldContinue(_variables[loopWidget.loopVariable]))
		{
			_lineIndex = loopWidget.lineIndex;
		}
		else
		{
			_variables.Remove(loopWidget.loopVariable);
			_loopStack.Pop();
		}
	}

	private void ParseVariableDeclaration(string line)
	{
		var varRegex = new Regex(VariableDeclarationPattern);
		var match = varRegex.Match(line.Trim());
		var variableName = match.Groups["variableName"].ToString().ToLower();
		var initialValue = System.Convert.ToSingle(match.Groups["initialValue"].ToString());

		if (_variables.ContainsKey(variableName))
		{
			throw new System.ArgumentException(string.Format("Duplicate declaration of variable {0}: {1}",
															 variableName, line));
		}

		_variables[variableName] = initialValue;
	}

	private void ParseLoopBegin(string line)
	{
		var loopRegex = new Regex(LoopRegexPattern);
		var match = loopRegex.Match(line.Trim());

		if (!match.Success)
		{
			throw new System.FormatException(line);
		}

		var loopVariableName = match.Groups["loopVarName"].ToString();
		var loopVariableValue = System.Convert.ToSingle(match.Groups["loopStart"].ToString());
		var loopLimit = System.Convert.ToSingle(match.Groups["loopEnd"].ToString());

		var capturedDelay = match.Groups["delay"].ToString();
		var delay = string.IsNullOrEmpty(capturedDelay) ? 0f : EE.Evaluate(capturedDelay, _variables);

		var capturedStep = match.Groups["loopStep"].ToString();
		var step = string.IsNullOrEmpty(capturedStep) ? 1f : EE.Evaluate(capturedStep, _variables);

		_variables[loopVariableName] = loopVariableValue;

		var loopWidget = new LoopWidget();

		loopWidget.loopVariable = loopVariableName;
		loopWidget.limit = loopLimit;
		loopWidget.lineIndex = _lineIndex;
		loopWidget.step = step;

		_loopDelay += delay;

		_loopStack.Push(loopWidget);
	}
}
