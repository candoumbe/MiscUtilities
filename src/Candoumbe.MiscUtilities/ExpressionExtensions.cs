﻿using Utilities;

namespace System.Linq.Expressions
{
    /// <summary>
    /// Extensions method
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Creates a expression that f as <c>f(x) => true</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static Expression<Func<T, bool>> True<T>() => _ => true;

        /// <summary>
        /// Creates an expression f so that <c>f(x) => x</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, T>> Identity<T>() => x => x;

        /// <summary>
        /// Combines two expressions <paramref name="first"/> and <paramref name="second"/> as defined in http://en.wikipedia.org/wiki/Function_composition.
        /// </summary>
        /// <typeparam name="T1">Input type of the first function</typeparam>
        /// <typeparam name="T2">Output type of the second function</typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="first">the first expression</param>
        /// <param name="second">An expressions</param>
        /// <returns>The combined expression</returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="first"/> or <paramref name="second"/> is <see langword="null"/>.</exception>
        public static Expression<Func<T1, T3>> Compose<T1, T2, T3>(this Expression<Func<T1, T2>> first, Expression<Func<T2, T3>> second)
        {
#if NET
            ArgumentNullException.ThrowIfNull(first);

            ArgumentNullException.ThrowIfNull(second);
#else
            if (first is null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second is null)
            {
                throw new ArgumentNullException(nameof(second));
            }
#endif

            ParameterExpression param = Expression.Parameter(typeof(T1), "param");

            Expression newFirst = new ReplaceVisitor(first.Parameters[0], param).Visit(first.Body);

            Expression newSecond = new ReplaceVisitor(second.Parameters[0], newFirst).Visit(second.Body);

            return Expression.Lambda<Func<T1, T3>>(newSecond, param);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>&amp;&amp;</c>
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="first">the first expression</param>
        /// <param name="second">the second expression</param>
        /// <returns>A new expression that represents <c>(T x) => f &amp;&amp; g</c></returns>
        /// <exception cref="ArgumentNullException">either<paramref name="first"/> or <paramref name="second"/> is <see langword="null"/>.</exception>
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
#if NET
            ArgumentNullException.ThrowIfNull(first);

            ArgumentNullException.ThrowIfNull(second);
#else
            if (first is null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second is null)
            {
                throw new ArgumentNullException(nameof(second));
            }
#endif

            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(first.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(first.Body);

            ReplaceVisitor rightVisitor = new(second.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(second.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>||</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns>A new expression that represents <c>(T x) => f || g</c></returns>
        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(expr1.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(expr1.Body);

            ReplaceVisitor rightVisitor = new(expr2.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>|</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns>A new expression that represents <c>(T x) => f | g</c></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(expr1.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(expr1.Body);

            ReplaceVisitor rightVisitor = new(expr2.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.Or(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>&amp;</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <c>(T x) => f &amp; g</c></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(f.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new(g.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.And(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>&lt;</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <c>(T x) => f &lt; g</c></returns>
        public static Expression<Func<T, bool>> LessThan<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(f.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new(g.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.LessThan(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>&gt;</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <c>(T x) => f > g</c></returns>
        public static Expression<Func<T, bool>> GreaterThan<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(f.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new(g.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>&gt;=</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <c>(T x) => f >= g</c></returns>
        public static Expression<Func<T, bool>> GreaterThanOrEqual<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(f.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new(g.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>&lt;=</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <c>(T x) => f &lt;= g</c></returns>
        public static Expression<Func<T, bool>> LessThanOrEqual<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(f.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new(g.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <c>==</c> operator
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <c>(T x) => f == g</c></returns>
        public static Expression<Func<T, bool>> Equal<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new(f.Parameters[0], parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new(g.Parameters[0], parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.Equal(left, right), parameter);
        }
    }
}
