//
//	Regular Expression Engine C# Sample Application
//	2006, by Leniel Braz de Oliveira Macaferi & Wellington Magalhães Leite.
//
//  UBM's Computer Engineering - 7th term [http://www.ubm.br/]
//  
//  This program sample was developed and turned in as a term paper for Lab. of
//  Compilers Construction. It was based on the source code provided by Eli Bendersky
//  [http://eli.thegreenplace.net/] and is provided "as is" without warranty.
//

using System;
using System.Text;

namespace RegularExpressionEngine
{
  /// <summary>
  /// Implements a parser for a given regular expression.
  /// </summary>
  class RegexParser
  {
    private string data;
    private int next;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    private void Init(string data)
    {
      this.data = Preprocess(data);
      next = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private char Peek()
    {
      return (next < data.Length) ? data[next] : '\0';
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private char Pop()
    {
      char cur = Peek();

      if(next < data.Length)
        ++next;

      return cur;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int GetPos()
    {
      return next;
    }

    /// <summary>
    /// Generates concatenation chars ('.') where appropriate.
    /// </summary>
    /// <param name="in"></param>
    /// <returns></returns>
    private string Preprocess(string @in)
    {
      StringBuilder @out = new StringBuilder();

      CharEnumerator c, up;
      c = @in.GetEnumerator();
      up = @in.GetEnumerator();

      up.MoveNext();

      // In this loop c is the current char of in, up is the next one.
      while(up.MoveNext())
      {
        c.MoveNext();

        @out.Append(c.Current);

        if((char.IsLetterOrDigit(c.Current) || c.Current == ')' || c.Current == '*' ||
          c.Current == '?') && (up.Current != ')' && up.Current != '|' &&
          up.Current != '*' && up.Current != '?'))
          @out.Append('.');
      }

      // Don't forget the last char...
      if(c.MoveNext())
        @out.Append(c.Current);

      return @out.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="offset"></param>
    private static void PrintTree(ParseTree node, int offset)
    {
      if(node == null)
        return;

      for(int i = 0; i < offset; ++i)
        Console.Write(" ");

      switch(node.type)
      {
        case ParseTree.NodeType.Chr:
          Console.WriteLine(node.data);
          break;
        case ParseTree.NodeType.Alter:
          Console.WriteLine("|");
          break;
        case ParseTree.NodeType.Concat:
          Console.WriteLine(".");
          break;
        case ParseTree.NodeType.Question:
          Console.WriteLine("?");
          break;
        case ParseTree.NodeType.Star:
          Console.WriteLine("*");
          break;
      }

      Console.Write("");

      PrintTree(node.left, offset + 8);
      PrintTree(node.right, offset + 8);
    }

    /// <summary>
    /// RD parser
    /// char ::= alphanumeric character (letter or digit)
    /// </summary>
    /// <returns></returns>
    private ParseTree Chr()
    {
      char data = Peek();

      if(char.IsLetterOrDigit(data) || data == '\0')
      {
        return new ParseTree(ParseTree.NodeType.Chr, this.Pop(), null, null);
      }
      else
      {
        Console.WriteLine("Parse error: expected alphanumeric, got {0} at #{1}",
        Peek(), GetPos());

        Console.ReadKey();

        Environment.Exit(1);

        return null;
      }
    }

    /// <summary>
    /// atom ::= char | '(' expr ')'
    /// </summary>
    /// <returns></returns>
    private ParseTree Atom()
    {
      ParseTree atomNode;

      if(Peek() == '(')
      {
        Pop();

        atomNode = Expr();

        if(Pop() != ')')
        {
          Console.WriteLine("Parse error: expected ')'");

          Environment.Exit(1);
        }
      }
      else
        atomNode = Chr();

      return atomNode;
    }

    /// <summary>
    /// rep ::= atom '*' | atom '?' | atom
    /// </summary>
    /// <returns></returns>
    private ParseTree Rep()
    {
      ParseTree atomNode = Atom();

      if(Peek() == '*')
      {
        Pop();

        ParseTree repNode = new ParseTree(ParseTree.NodeType.Star, null, atomNode, null);

        return repNode;
      }
      else if(Peek() == '?')
      {
        Pop();

        ParseTree repNode = new ParseTree(ParseTree.NodeType.Question, ' ', atomNode, null);

        return repNode;
      }
      else
        return atomNode;
    }

    /// <summary>
    /// concat ::= rep . concat | rep
    /// </summary>
    /// <returns></returns>
    private ParseTree Concat()
    {
      ParseTree left = Rep();

      if(Peek() == '.')
      {
        Pop();

        ParseTree right = Concat();

        ParseTree concatNode = new ParseTree(ParseTree.NodeType.Concat, null, left, right);

        return concatNode;
      }
      else
        return left;
    }

    /// <summary>
    /// expr   ::= concat '|' expr | concat
    /// </summary>
    /// <returns></returns>
    private ParseTree Expr()
    {
      ParseTree left = Concat();

      if(Peek() == '|')
      {
        Pop();

        ParseTree right = Expr();

        ParseTree exprNode = new ParseTree(ParseTree.NodeType.Alter, null, left, right);

        return exprNode;
      }
      else
        return left;
    }

    /// <summary>
    /// The main entry point of the Console Application
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
      if(args.Length != 3)
      {
        Console.WriteLine("Call with the regex as an argument.");

        Environment.Exit(1);
      }

      RegexParser myRegexParser = new RegexParser();

      // Passing the regex to be preprocessed.
      myRegexParser.Init(args[1]);

      // Creating a parse tree with the preprocessed regex
      ParseTree parseTree = myRegexParser.Expr();

      // Checking for a string termination character after
      // parsing the regex
      if(myRegexParser.Peek() != '\0')
      {
        Console.WriteLine("Parse error: unexpected char, got {0} at #{1}",

        myRegexParser.Peek(), myRegexParser.GetPos());

        Environment.Exit(1);
      }

      PrintTree(parseTree, 1);

      NFA nfa = NFA.TreeToNFA(parseTree);

      nfa.Show();

      DFA dfa = SubsetMachine.SubsetConstruct(nfa);

      dfa.Show();

      Console.Write("\n\n");

      Console.Write("Result: {0}", dfa.Simulate(args[2]));

      Console.ReadKey();
    }

  }
}