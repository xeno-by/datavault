using System;
using System.Linq.Expressions;
using DataVault.Core.Helpers.Reflection;

namespace DataVault.Core.Helpers
{
    public static class LinqHelpers
    {
        public static Object Eval(this Expression e)
        {
            var delegateType = typeof(Func<>).XMakeGenericType(e.Type);
            var rvalue_prepared = Expression.Lambda(delegateType, e);
            return rvalue_prepared.Compile().DynamicInvoke();
        }
    }
}
