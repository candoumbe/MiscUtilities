using System.Linq.Expressions;

namespace Utilities
{
    internal class ReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression _from, _to;

        public ReplaceVisitor(Expression from, Expression to)
        {
            _from = from;
            _to = to;
        }        public override Expression Visit(Expression node) => node == _from ? _to : base.Visit(node);
    }
}
