using System.Linq.Expressions;

namespace Utilities
{
    /// <summary>
    /// <see cref="ExpressionVisitor"/> implementation that can be used to replace an expression with an other
    /// </summary>
    /// <remarks>
    /// Builds a new <see cref="ReplaceVisitor"/> instance that can be used to replace <paramref name="from"/> expression with <paramref name="to"/>
    /// </remarks>
    /// <param name="from">The expression to replace</param>
    /// <param name="to">The expression which will replace <paramref name="from"/></param>
    public class ReplaceVisitor(Expression from, Expression to) : ExpressionVisitor
    {
        private readonly Expression _from = from, _to = to;

        ///<inheritdoc/>
        public override Expression Visit(Expression node) => node == _from ? _to : base.Visit(node);
    }
}
