using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public abstract class ASTNode { }

    public class CommandNode : ASTNode
    {
        public TokenType CommandType { get; }
        public string Argument { get; }

        public CommandNode(TokenType commandType, string argument)
        {
            CommandType = commandType;
            Argument = argument;
        }

        public override string ToString() => $"{CommandType} {Argument}";
    }

    public class RepeatNode : ASTNode
    {
        public string Count { get; }
        public List<ASTNode> Body { get; }

        public RepeatNode(string count, List<ASTNode> body)
        {
            Count = count;
            Body = body;
        }

        public override string ToString()
        {
            string inner = string.Join("\n  ", Body.Select(b => b.ToString()));
            return $"repeat {Count} [\n  {inner}\n]";
        }
    }

}
