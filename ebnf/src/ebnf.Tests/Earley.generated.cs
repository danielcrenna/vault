using System;
using System.Collections.Generic;

namespace Calc
{
	/// <summary>
	/// An Earley parser for the Calc language.
	/// <see cref="http://en.wikipedia.org/wiki/Earley_parser" />
	/// </summary>
	public class CalcParser
	{
		public IEnumerable<CalcSyntaxTree> Parse(string input)
		{
			throw new NotImplementedException();
		}
	}
}

// Syntax Tree
namespace Calc
{
	public partial class CalcSyntaxTree
	{

	}
}

// Terminals
namespace Calc
{
	public partial class Terminal
	{
		private static List<string> _terminals;

		static Terminal()
		{
			_terminals = new List<string>();
			_terminals.Add("0"); 
			_terminals.Add("1"); 
			_terminals.Add("2"); 
			_terminals.Add("3"); 
			_terminals.Add("4"); 
			_terminals.Add("5"); 
			_terminals.Add("6"); 
			_terminals.Add("7"); 
			_terminals.Add("8"); 
			_terminals.Add("9"); 
		}

		public static bool IsTerminal(string input)
		{
			return _terminals.Contains(input);
		}

		public static bool IsTerminal(params char[] input)
		{
			return IsTerminal(new string(input));
		}
	}
}

// Expressions
namespace Calc
{
	public partial class Expression
	{

	}

	public partial class DigitExpression : Expression
	{

	}
	public partial class LiteralExpression : Expression
	{

	}
	public partial class ExpExpression : Expression
	{

	}
}

