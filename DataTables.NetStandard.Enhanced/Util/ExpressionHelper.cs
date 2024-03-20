using System;
using System.Linq.Expressions;

namespace DataTables.NetStandard.Enhanced.Util
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// Creates a constant filter expression of the given <paramref name="value"/> and converts the type to the given <paramref name="type"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        internal static Expression CreateConstantFilterExpression(object value, Type type)
        {
            // The value is converted to anonymous function only returning the value itself.
            Expression<Func<object>> valueExpression = () => value;

            // Afterwards only the body of the function, which is the value, is converted to the delivered type.
            // Therefore no Expression.Constant is necessary which lead to memory leaks, because EFCore caches such constants.
            // Caching constants is not wrong, but creating constants of dynamic search values is wrong.
            return Expression.Convert(valueExpression.Body, type);
        }
    }
}
