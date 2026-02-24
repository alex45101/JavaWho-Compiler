using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JavaWhoCompiler
{
    public abstract class AST
    {
        public AST[] Nodes { get; init; }

        public AST(int nodeAmount)
        {
            Nodes = new AST[nodeAmount];
        }
    }

    public class PrimaryExpression : AST
    {
        public PrimaryExpression(int nodeAmount) : base(nodeAmount)
        {
        }
    }

    public class CallExpression : AST
    {
        public CallExpression(int nodeAmount) : base(nodeAmount)
        {
        }
    }

    public class MultiplyExpression : AST
    {
        public MultiplyExpression(int nodeAmount) : base(nodeAmount)
        {
        }
    }

    public class AddExpresion : AST
    {
        public AddExpresion(int nodeAmount) : base(nodeAmount)
        {
        }
    }

    public class ComparisonExpression : AST
    {
        public ComparisonExpression(int nodeAmount) : base(nodeAmount)
        {
        }
    }

    public class EqualityExpression : AST
    {
        public EqualityExpression(int nodeAmount) : base(nodeAmount)
        {
        }
    }

    public class Expression : AST
    {
        public Expression(int nodeAmount) : base(nodeAmount)
        {

        }
    }


    public static class Parser
    {
        public static AST Parse(IEnumerable<IToken> tokens)
        {
            throw new NotImplementedException();
        }
    }
}
