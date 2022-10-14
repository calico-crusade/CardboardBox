using System.Linq.Expressions;
using System.Reflection;

namespace CardboardBox.Database.Generation
{
	public static class Extensions
	{
        public static PropertyInfo GetPropertyInfo<TSource, TProp>(this Expression<Func<TSource, TProp>>? propertyLambda)
        {
            if (propertyLambda == null) throw new ArgumentNullException(nameof(propertyLambda));

            var type = typeof(TSource);

            if (propertyLambda.Body is not MemberExpression member)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (propInfo.ReflectedType != null &&
                type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
    }
}
