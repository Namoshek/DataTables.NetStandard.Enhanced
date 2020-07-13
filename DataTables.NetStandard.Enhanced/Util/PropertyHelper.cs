using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.NetStandard.Enhanced.Util
{
    /// <summary>
    /// Provides useful methods to work with expressions and properties.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public static class PropertyHelper<TEntity>
    {
        /// <summary>
        /// Retrieve the <see cref="PropertyInfo"/> of a property using the given <paramref name="selector"/>.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty<TValue>(Expression<Func<TEntity, TValue>> selector)
        {
            Expression body = selector;

            if (body is LambdaExpression)
            {
                body = ((LambdaExpression)body).Body;
            }

            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (PropertyInfo)((MemberExpression)body).Member;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Retrieve a <see cref="MemberExpression"/> from a property selector, which can also be a nested one.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            
            if (expression is LambdaExpression lambdaExpression)
            {
                if (lambdaExpression.Body is MemberExpression memberBody)
                {
                    return memberBody;
                }
                
                if (lambdaExpression.Body is UnaryExpression unaryBody)
                {
                    return (MemberExpression)unaryBody.Operand;
                }
            }

            return null;
        }
    }
}
