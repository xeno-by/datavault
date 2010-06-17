using System;
using System.Reflection;
using System.Reflection.Emit;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Codegen;
using DataVault.Core.Helpers.Reflection;
using DataVault.Core.Helpers.Reflection.Shortcuts;
using System.Linq;
using DataVault.Core.Helpers;

namespace DataVault.Core.Helpers.StronglyTypedReflection
{
    // todo. STR is awesome but it needs certain love to become very awesome
    // 1) split into partial classes: Method and Slot
    // 2) GetSlotProxy -> old GetSlot, GetSlot/SetSlot -> old GetSlot.Get(), GetSlot.Set()
    // 3) change arg binding mode into supporting inheritance, _ will depict custom conversions
    // 4) GetXXX -> e.g. for ToString()
    //      * for Object params it returns immediately bound stuff, e.g. () => String
    //      * for Type - stuff that requires "this", e.g. (Object) => String
    // 5) Strongly type case 4b

    public static partial class Api
    {
        public static Slot<T> GetSlot<T>(this Object target, String name)
        {
            return target.GetSlot<T>(target.GetType(), name);
        }

        public static Slot<T> GetSlot<T>(this Object target, String name, BindingFlags flags)
        {
            return target.GetSlot<T>(target.GetType(), name, flags);
        }

        public static Slot<T> GetSlot<T>(this Type t, String name)
        {
            return ((Object)null).GetSlot<T>(t, name);
        }

        public static Slot<T> GetSlot<T>(this Type t, String name, BindingFlags flags)
        {
            return ((Object)null).GetSlot<T>(t, name, flags);
        }

        public static Slot<T> GetSlot<T>(this Object target, Type t, String name)
        {
            return target.GetSlot<T>(t, name, BF.All | BindingFlags.FlattenHierarchy);
        }

        public static Slot<T> GetSlot<T>(this Object target, Type t, String name, BindingFlags flags)
        {
            var f = t.GetField(name, flags);
            var p = t.GetProperties(flags).Where(pi => pi.Name == name && pi.GetIndexParameters().IsEmpty()).SingleOrDefault();
            (f != null && p != null).AssertFalse();

            if (f == null && p == null)
            {
                if ((flags & BindingFlags.NonPublic) != 0)
                {
                    // try to find private slots in base classes
                    var private_fs = t.LookupInheritanceChain().Select(bt => bt.GetField(name, flags));
                    var private_ps = t.LookupInheritanceChain().Select(bt => bt.GetProperties(flags).Where(pi => pi.Name == name && pi.GetIndexParameters().IsEmpty()).SingleOrDefaultDontCrash());

                    f = private_fs.SingleOrDefaultDontCrash(private_f => private_f != null);
                    p = private_ps.SingleOrDefaultDontCrash(private_f => private_f != null);
                    (f != null && p != null).AssertFalse();

                    if (f == null && p == null)
                    {
                        // if this doesn't help - we give up
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            var ret = f != null ? f.FieldType : p.PropertyType;
            if (typeof(T) == typeof(_))
            {
                // validation ok
            }
            else if (typeof(T).SameMetadataToken(typeof(_<>)))
            {
                var t_unwrapped = typeof(T).XGetGenericArguments().Single();
                // todo. this implementation is very incomplete
                t_unwrapped.IsAssignableFrom(ret).AssertTrue();
            }
            else
            {
                (ret == typeof(T)).AssertTrue();
            }

            if (f != null)
            {
                (f.IsStatic ^ target != null).AssertTrue();

                if (f.IsStatic)
                {
                    var getter_il = new DynamicMethod(
                        String.Format("<>__get__{0}__decltype:{1}__slottype:{2}", name, t.FullName, typeof(T).FullName),
                        typeof(T), new Type[0], t, true); // nb! true is essential for accessing private stuff
                    getter_il.il().ldfld(f).ret();
                    var getter = (Func<T>)getter_il.CreateDelegate(typeof(Func<T>));

                    var setter_il = new DynamicMethod(
                        String.Format("<>__set__{0}__decltype:{1}__slottype:{2}", name, t.FullName, typeof(T).FullName),
                        typeof(void), new[] { typeof(T) }, t, true); // nb! true is essential for accessing private stuff
                    setter_il.DefineParameter(1, ParmA.None, "value");
                    setter_il.il().ldarg(0).stfld(f).ret();
                    var setter = (Action<T>)setter_il.CreateDelegate(typeof(Action<T>));

                    return new Slot<T>(getter, setter);
                }
                else
                {
                    var getter_il = new DynamicMethod(
                        String.Format("<>__get__{0}__decltype:{1}__slottype:{2}", name, t.FullName, typeof(T).FullName),
                        typeof(T), t.MkArray(), t, true); // nb! true is essential for accessing private stuff
                    getter_il.DefineParameter(1, ParmA.None, "this");
                    getter_il.il().ldarg(0).ldfld(f).ret();
                    var getter = (Func<T>)getter_il.CreateDelegate(typeof(Func<T>), target);

                    var setter_il = new DynamicMethod(
                        String.Format("<>__set__{0}__decltype:{1}__slottype:{2}", name, t.FullName, typeof(T).FullName),
                        typeof(void), new[] { t, typeof(T) }, t, true); // nb! true is essential for accessing private stuff
                    setter_il.DefineParameter(1, ParmA.None, "this");
                    setter_il.DefineParameter(2, ParmA.None, "value");
                    setter_il.il().ldarg(0).ldarg(1).stfld(f).ret();
                    var setter = (Action<T>)setter_il.CreateDelegate(typeof(Action<T>), target);

                    return new Slot<T>(getter, setter);
                }
            }
            else
            {
                p.AssertNotNull();

                if (p.IsStatic())
                {
                    var getter = t.GetMethod<Func<T>>("get_" + name, flags);
                    var setter = t.GetMethod<Action<T>>("set_" + name, flags);
                    return new Slot<T>(getter, setter);
                }
                else
                {
                    var getter = target.GetMethod<Func<T>>(t, "get_" + name, flags);
                    var setter = target.GetMethod<Action<T>>(t, "set_" + name, flags);
                    return new Slot<T>(getter, setter);
                }
            }
        }
    }
}