using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataVault.Core.Helpers.Reflection;

namespace DataVault.Core.Helpers.Assertions
{
    public static class AssertionHelper
    {
        public static T AssertNotNull<T>(this T obj)
            where T : class
        {
            if (obj == null)
            {
                throw new AssertionFailedException("This should be not null");
            }

            return obj;
        }

        public static T? AssertNotNull<T>(this T? obj)
            where T : struct
        {
            if (obj == null)
            {
                throw new AssertionFailedException("This should be not null");
            }

            return obj;
        }

        public static T MightBeNull<T>(this T obj)
            where T : class
        {
            return obj;
        }

        public static T? MightBeNull<T>(this T? obj)
            where T : struct
        {
            return obj;
        }

        public static T AssertEmpty<T>(this T obj)
            where T : IEnumerable
        {
            if (obj.IsNotEmpty())
            {
                throw new AssertionFailedException("This should be empty");
            }

            return obj;
        }

        public static T MigthBeEmpty<T>(this T obj)
            where T : IEnumerable
        {
            return obj;
        }

        public static T AssertNeitherNullNorEmpty<T>(this T obj)
            where T : IEnumerable
        {
            if (obj.IsNullOrEmpty())
            {
                throw new AssertionFailedException("This should be neither null nor empty");
            }

            return obj;
        }

        public static T MigthBeNullOrEmpty<T>(this T obj)
            where T : IEnumerable
        {
            return obj;
        }

        public static T AssertNull<T>(this T obj)
            where T : class
        {
            if (obj != null)
            {
                throw new AssertionFailedException("This should be null");
            }

            return obj;
        }

        public static T? AssertNull<T>(this T? obj)
            where T : struct
        {
            if (obj != null)
            {
                throw new AssertionFailedException("This should be null");
            }

            return obj;
        }

        public static bool AssertFalse(this bool obj)
        {
            if (obj)
            {
                throw new AssertionFailedException("This should be false");
            }

            return false;
        }

        public static bool AssertTrue(this bool obj)
        {
            if (!obj)
            {
                throw new AssertionFailedException("This should be true");
            }

            return true;
        }

        public static Exception Fail()
        {
            throw new AssertionFailedException("This should not happen");
        }

        public static Exception Fail(Exception ex)
        {
            throw new AssertionFailedException("This should not happen", ex);
        }

        public static void Success()
        {
        }

        public static T AssertCast<T>(this Object o)
        {
            try
            {
                if (typeof(T).SameMetadataToken(typeof(IEnumerable<>)))
                {
                    if (o == null)
                    {
                        return (T)o;
                    }
                    else
                    {
                        // when called for T ::= IEnumerable<E>
                        // this method effectively becomes a shortcut for
                        // o.AssertCast<IEnumerable>.Cast<E>

                        var linqCast = typeof(Enumerable).GetMethod("Cast");
                        linqCast = linqCast.MakeGenericMethod(typeof(T).GetEnumerableElement());
                        return (T)linqCast.Invoke(null, o.MkArray());
                    }
                }
                else
                {
                    return (T)o;
                }
            }
            catch (InvalidCastException ice)
            {
                throw new AssertionFailedException(String.Format(
                                                       "Object '{0}' was expected to be of type '{1}' but had type '{2}'", 
                                                       o, typeof(T), o == null ? "null" : o.GetType().ToString()), ice);
            } 
        }

        // use this signature when an argument is enumerable, 
        // but it's undesirable to use the following signature: 
        // IEnumerable<T> AssertCast<T>(this IEnumerable e)
        public static T AssertCastSequenceAsAWhole<T>(this Object o)
        {
            return o.AssertCast<T>();
        }

        public static IEnumerable<T> AssertCast<T>(this IEnumerable e)
        {
            try
            {
                return e == null ? null : e.Cast<T>().ToArray();
            }
            catch (InvalidCastException ice)
            {
                throw new AssertionFailedException(String.Format(
                                                       "Object '{0}' was expected to be of type IEnumerable<'{1}'> but had type '{2}'",
                                                       e, typeof(T), e == null ? "null" : e.GetType().ToString()), ice);
            }
        }

        public static Type AssertCast<T>(this Type t)
        {
            // todo. this implementation is very incomplete
            if (typeof(T).IsAssignableFrom(t))
            {
                return t;
            }
            else
            {
                throw new AssertionFailedException(String.Format(
                                                       "Type '{0}' was expected to be convertible to type '{1}'",
                                                       t, typeof(T)));
            }
        }
    }
}