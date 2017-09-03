using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Token
{
	public readonly string asString;
	public readonly bool isOperator;
	public readonly float value;
	public readonly Func<float, float, float> operation;
	public readonly bool isBracket;
	public readonly int precedence;

	public Token(string asString, bool isOperator, float value, Func<float, float, float> operation, bool isBracket = false)
	{
		this.asString = asString;
		this.isOperator = isOperator;
		this.value = value;
		this.operation = operation;
		this.isBracket = isBracket;
		this.precedence = (int) value;
	}

	public static Token Operator(string sign, Func<float, float, float> operation, float precedence, bool isBracket = false)
	{
		return new Token(sign, true, precedence, operation, isBracket);
	}

	public static Token Add() { return Operator("+", (a, b) => a + b, 5f); }
	public static Token Sub() { return Operator("-", (a, b) => a - b, 5f); }
	public static Token Div() { return Operator("/", (a, b) => a / b, 10f); }
	public static Token Mul() { return Operator("*", (a, b) => a * b, 10f); }
	public static Token Left() { return Operator("(", null, 0f, true); }
	public static Token Right() { return Operator(")", null, 0f, true); }
}

public static class TokenFactory
{
	//TODO: Be able to pass in custom conversions, value calculations &c
	//		(i.e. so that we can calculate the value of a token)
	public static Token CreateToken(bool isOperator, bool isVariable, bool negate, string str, Dictionary<string, float> variables = null)
	{
		if (isOperator)
		{
			return CreateOperator(str);
		}

		var value = isVariable ? variables[str] : Convert.ToSingle(str);
		if (negate)
		{
			value *= -1f;
		}

		return new Token(str, false, value, null);
	}

	public static Token CreateOperator(string str)
	{
		switch (str)
		{
			case "+": return Token.Add();
			case "-": return Token.Sub();
			case "/": return Token.Div();
			case "*": return Token.Mul();
			case "(": return Token.Left();
			case ")": return Token.Right();
		}

		return null;
	}
}

public class EE
{
	public const string OPERATORS = @"()+-/*";
	public const string NUMBERS = @"0123456789.";

	public static float Evaluate(string expression, Dictionary<string, float> variables)
	{
		return EvaluateRpnTokens(GetRPNTokens(expression, variables));
	}

	public static float EvaluateRpnTokens(List<Token> rpnTokens)
	{
		var result = 0f;
		var stack = new Stack<float>();

		for (int i = 0; i < rpnTokens.Count; ++i)
		{
			var token = rpnTokens[i];
			if (!token.isOperator)
			{
				if (i < rpnTokens.Count - 1)
				{
					stack.Push(token.value);
				}
				else
				{
					result += token.value;
				}
			}
			else
			{
				var operandB = stack.Pop();
				var operandA = stack.Pop();
				var part = token.operation(operandA, operandB);
				if (i < rpnTokens.Count - 1)
				{
					stack.Push(part);
				}
				else
				{
					result += part;
				}
			}
		}

		return result;
	}

	public static List<Token> GetRPNTokens(string expression, Dictionary<string, float> variables)
	{
		var inTokens = Parse(expression, variables);
		var outTokens = new List<Token>();
		var opStack = new Stack<Token>();

		for (int i = 0; i < inTokens.Count; ++i)
		{
			var token = inTokens[i];
			if (!token.isOperator) { outTokens.Add(token); }

			if (token.isOperator && !token.isBracket)
			{
				while (opStack.Count > 0 && opStack.Peek() != null && !opStack.Peek().isBracket && opStack.Peek().precedence >= token.precedence)
				{
					outTokens.Add(opStack.Pop());
				}

				opStack.Push(token);
			}

			if (token.asString == "(")
			{ 
				opStack.Push(token);
			}

			if (token.asString == ")")
			{
				while (opStack.Count > 0 && opStack.Peek() != null && opStack.Peek().asString != "(") 
				{
					outTokens.Add(opStack.Pop());
				}
				opStack.Pop();	// final left bracket)
			}
		}

		while (opStack.Count > 0)
		{
			outTokens.Add(opStack.Pop());
		}

		return outTokens;
	}

	public static List<Token> Parse(string expression, Dictionary<string, float> variables)
	{
		var tokens = new List<Token>();
		if (!string.IsNullOrEmpty(expression))
		{
			expression = RemoveWhiteSpace(expression);

			int start = 0, onePastEnd = 1;
			var exprLength = expression.Length;
			var hadLeftOperand = false;
			var unaryMinus = false;

			while (start < expression.Length && onePastEnd <= exprLength)
			{
				if (expression[start] == '-' && !hadLeftOperand)
				{
					unaryMinus = true;
					start++;
					onePastEnd = start + 1;
				}

				var isOperator = IsOperator(expression[start]);
				hadLeftOperand = !isOperator || (expression[start] == ')');
				var isVariable = !isOperator && !IsNumber(expression[start]);

				if (!isOperator)
				{
					while (onePastEnd < exprLength && !IsOperator(expression[onePastEnd]))
					{
						isVariable |= !IsNumber(expression[onePastEnd]);
						++onePastEnd;
					}
				}

				tokens.Add(TokenFactory.CreateToken(isOperator, isVariable, unaryMinus, expression.Substring(start, onePastEnd - start), variables));

				if (!isOperator)
				{
					unaryMinus = false;
				}

				start = onePastEnd;
				onePastEnd++;
			}
		}

		return tokens;
	}

	public static bool IsNumber(char chr)
	{
		foreach (var num in NUMBERS)
		{
			if (num == chr)
			{
				return true;
			}
		}

		return false;
	}

	public static bool IsOperator(char chr)
	{
		foreach (var op in OPERATORS)
		{
			if (op == chr)
			{
				return true;
			}
		}

		return false;
	}

	public static string RemoveWhiteSpace(string expression)
	{
		return Regex.Replace(expression, @"\s", "");
	}
}
