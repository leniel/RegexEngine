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

using input = System.Char;

namespace RegularExpressionEngine
{
  /// <summary>
  /// Parse tree
  /// </summary>
  class ParseTree
  {
    public enum NodeType
    {
      Chr,
      Star,
      Question,
      Alter,
      Concat
    }
    
    public NodeType type;
    public input? data;
    public ParseTree left;
    public ParseTree right;

    public ParseTree(NodeType type_, input? data_, ParseTree left_, ParseTree right_)
    {
      type = type_;
      data = data_;
      left = left_;
      right = right_;
    }
  }
}