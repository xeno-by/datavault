using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DataVault.Core.Helpers.Codegen
{
    public class FromLambda
    {
        public static MethodInfo Getter<T0, TProperty>(Expression<Func<T0, TProperty>> marker)
        {
            var pi = ((MemberExpression)marker.Body).Member as PropertyInfo;
            return pi.GetGetMethod();
        }

        public static MethodInfo Setter<T0, TProperty>(Expression<Func<T0, TProperty>> marker)
        {
            var pi = (marker.Body as MemberExpression).Member as PropertyInfo;
            // expressions cannot use the assignment operator
            // thus we need to check out existance of setter manually
            if (!pi.CanWrite) throw new InvalidOperationException(String.Format("Property {0}::{1} has no setter anymore", pi.DeclaringType, pi.Name));
            return pi.GetSetMethod();
        }

        public static MethodInfo Method(Expression<Action> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0>(Expression<Action<T0>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0, T1>(Expression<Action<T0, T1>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0, T1, T2>(Expression<Action<T0, T1, T2>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0, T1, T2, T3>(Expression<Action<T0, T1, T2, T3>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<TResult>(Expression<Func<TResult>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0, TResult>(Expression<Func<T0, TResult>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0, T1, TResult>(Expression<Func<T0, T1, TResult>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0, T1, T2, TResult>(Expression<Func<T0, T1, T2, TResult>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }

        public static MethodInfo Method<T0, T1, T2, T3, TResult>(Expression<Func<T0, T1, T2, T3, TResult>> marker)
        {
            return ((MethodCallExpression)marker.Body).Method;
        }
    }
}