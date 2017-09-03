using UnityEngine;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

public class ExpressionEvaluatorTests
{
	[Test]
	public void EvalTest()
	{
		Eval("1 + 1", 2f);
		Eval("3 + (4 * 2)", 11f);	// 3 4 2 * +
		Eval("(3 + 4) * 2", 14f);	// 3 4 + 2 *
		Eval("3 + 4 * 2", 11f);
		Eval("((4 / 2) * (3 - 1))", 4f);
		Eval("(4 / (2 * 3) - 1)", -1f / 3f);
	}

	[Test]
	public void UnaryEdgeCasesTest()
	{
		Eval("1", 1f);
		Eval("5 + -1", 4f);
		Eval("-(3 * 2)", -6f);

		var variables = new Dictionary<string, float> { { "i", 10 } };
		Eval("3 * -i + 2", -28f, variables);
		Eval("-(3 * (10 - 8) / -i) + 2", -(3f * (10f - 8f) / (float) -variables["i"]) + 2f, variables);  
	}

	[Test]
	public void VarTest()
	{
		var variables = new Dictionary<string, float> { { "i", 10 } };
		Eval("3 * i + 2", 32f, variables);

		for (int i = 0; i < 10; ++i)
		{
			variables["i"] = i;
			Eval("0.2*i+(14/(3.5 - 9))", 0.2f * (float)i + (14f / (3.5f - 9f)), variables); 
		}
	}

	public void Eval(string expr, float expected, Dictionary<string, float> variables = null)
	{
		Assert.IsTrue(Mathf.Approximately(EE.Evaluate(expr, variables), expected), 
										string.Format("Expected: {0}, actual: {1}", expected, EE.Evaluate(expr, variables)));
	}

	[Test]
	public void RPNTest()
	{
		var expr = "1 + 1";
		var RPNTokens = EE.GetRPNTokens(expr, null).Select(token => token.asString).ToList();
		Assert.IsTrue(RPNTokens.Count == 3 && RPNTokens[0] == "1" && RPNTokens[1] == "1" && RPNTokens[2] == "+");

		expr = "3 + (4 * 2)";
		RPNTokens = EE.GetRPNTokens(expr, null).Select(token => token.asString).ToList();
		Assert.IsTrue(RPNTokens.Count == 5 &&
						RPNTokens[0] == "3" &&
						RPNTokens[1] == "4" &&
						RPNTokens[2] == "2" &&
						RPNTokens[3] == "*" &&
						RPNTokens[4] == "+");

		expr = "(3 + 4) * 2";
		RPNTokens = EE.GetRPNTokens(expr, null).Select(token => token.asString).ToList();
		Assert.IsTrue(RPNTokens.Count == 5 &&
						RPNTokens[0] == "3" &&
						RPNTokens[1] == "4" &&
						RPNTokens[2] == "+" &&
						RPNTokens[3] == "2" &&
						RPNTokens[4] == "*");
	}

	[Test]
	public void TokenParseTest1()
	{
		var expr = "1 + 1";
		var tokens = EE.Parse(expr, null)
						.Select(token => token.asString)
						.ToList();

		Assert.IsTrue(tokens.Count == 3 && tokens[0] == "1" && tokens[1] == "+" && tokens[2] == "1");

		expr = "(3+4 * 9 + 212 - 1.5)";
		tokens = EE.Parse(expr, null).Select(token => token.asString).ToList();

		Assert.IsTrue(tokens.Count == 11 &&
						tokens[0] == "(" &&
						tokens[1] == "3" &&
						tokens[2] == "+" &&
						tokens[3] == "4" &&
						tokens[4] == "*" &&
						tokens[5] == "9" &&
						tokens[6] == "+" &&
						tokens[7] == "212" &&
						tokens[8] == "-" &&
						tokens[9] == "1.5" &&
						tokens[10] == ")");
	}

	[Test]
	public void StripWhiteSpaceTest()
	{
		var expr = "1 + 1";
		var noWs = EE.RemoveWhiteSpace(expr);
		Assert.IsTrue(noWs == "1+1");

		expr = "	(12 + (4.5 * i) / 2)";
		noWs = EE.RemoveWhiteSpace(expr);
		Assert.IsTrue(noWs == "(12+(4.5*i)/2)");
	}
}
