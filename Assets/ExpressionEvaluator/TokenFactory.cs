using System;
using System.Collections.Generic;
public static class TokenFactory
{
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
