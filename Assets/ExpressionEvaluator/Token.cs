using System;
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
