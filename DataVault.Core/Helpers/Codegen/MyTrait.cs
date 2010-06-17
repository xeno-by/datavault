using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection.Shortcuts;
using DataVault.Core.Helpers.Reflection;

namespace DataVault.Core.Helpers.Codegen
{
    public static class MyTrait
    {
        public static ILGenerator il(this DynamicMethod mb)
        {
            return mb.GetILGenerator();
        }
        
        public static ILGenerator il(this MethodBuilder mb)
        {
            return mb.GetILGenerator();
        }

        public static ILGenerator il(this ConstructorBuilder cb)
        {
            return cb.GetILGenerator();
        }

        public static ILGenerator def_label(this ILGenerator il, out Label l)
        {
            return il.DefineLabel(out l);
        }

        public static ILGenerator def_local(this ILGenerator il, Type t, out LocalBuilder l)
        {
            return il.DefineLocal(t, out l);
        }

        public static void DefineProperty(this TypeBuilder t, String name, Type propertyType)
        {
            t.DefineProperty(name, propertyType, false);
        }

        public static void DefineProperty(
            this TypeBuilder t, String name, Type propertyType, out FieldBuilder backing)
        {
            MethodBuilder get, set;
            DefineProperty(t, name, propertyType, out get, out set, out backing);
        }

        public static void DefineProperty(this TypeBuilder t, String name, Type propertyType, bool autoImpl)
        {
            MethodBuilder get, set;
            if (autoImpl)
            {
                FieldBuilder backing;
                DefineProperty(t, name, propertyType, out get, out set, out backing);
            }
            else
            {
                DefineProperty(t, name, propertyType, out get, out set);
            }
        }

        public static void DefineProperty(
            this TypeBuilder t, String name, Type propertyType, out MethodBuilder get, out MethodBuilder set)
        {
            var prop = t.DefineProperty(name, PropA.None, propertyType, new Type[0]);
            get = t.DefineMethod("get_" + name, MA.PublicProp, propertyType, new Type[0]);
            set = t.DefineMethod("set_" + name, MA.PublicProp, null, new[] { propertyType });
            prop.SetGetMethod(get);
            prop.SetSetMethod(set);
        }

        public static void DefineProperty(
            this TypeBuilder t, String name, Type propertyType, out MethodBuilder get, out MethodBuilder set, out FieldBuilder backing)
        {
            t.DefineProperty(name, propertyType, out get, out set);

            backing = t.DefineField("_" + name.ToLower(), propertyType, FA.Private);
            get.il().ldarg(0).ldfld(backing).ret();
            set.il().ldarg(0).ldarg(1).stfld(backing).ret();
        }

        public static MethodBuilder DefineReadonlyProperty(
            this TypeBuilder t, String name, Type propertyType, bool autoImpl)
        {
            if (autoImpl)
            {
                FieldBuilder backing;
                return DefineReadonlyProperty(t, name, propertyType, out backing);
            }
            else
            {
                return DefineReadonlyProperty(t, name, propertyType);
            }
        }

        public static MethodBuilder DefineReadonlyProperty(
            this TypeBuilder t, String name, Type propertyType)
        {
            var vpp = t.DefineProperty(name, PropA.None, propertyType, new Type[0]);
            var get = t.DefineMethod("get_" + name, MA.PublicProp, propertyType, new Type[0]);
            vpp.SetGetMethod(get);

            return get;
        }

        public static MethodBuilder DefineReadonlyProperty(
            this TypeBuilder t, String name, Type propertyType, out FieldBuilder backing)
        {
            var get = t.DefineReadonlyProperty(name, propertyType);
            backing = t.DefineField("_" + name.ToLower(), propertyType, FA.Private);
            get.il().ldarg(0).ldfld(backing).ret();

            return get;
        }

        public static MethodBuilder DefineWriteonlyProperty(
            this TypeBuilder t, String name, Type propertyType, bool autoImpl)
        {
            if (autoImpl)
            {
                FieldBuilder backing;
                return DefineWriteonlyProperty(t, name, propertyType, out backing);
            }
            else
            {
                return DefineWriteonlyProperty(t, name, propertyType);
            }
        }

        public static MethodBuilder DefineWriteonlyProperty(
            this TypeBuilder t, String name, Type propertyType)
        {
            var vpp = t.DefineProperty(name, PropA.None, propertyType, new Type[0]);
            var set = t.DefineMethod("set_" + name, MA.PublicProp, null, new[] { propertyType });
            vpp.SetSetMethod(set);

            return set;
        }

        public static MethodBuilder DefineWriteonlyProperty(
            this TypeBuilder t, String name, Type propertyType, out FieldBuilder backing)
        {
            var set = t.DefineWriteonlyProperty(name, propertyType);
            backing = t.DefineField("_" + name.ToLower(), propertyType, FA.Private);
            set.il().ldarg(0).ldarg(1).stfld(backing).ret();

            return set;
        }

        public static void DefineAbstractProperty(this TypeBuilder t, String name, Type propertyType)
        {
            t.DefineAbstractProperty(name, propertyType, false);
        }

        public static void DefineAbstractProperty(
            this TypeBuilder t, String name, Type propertyType, out FieldBuilder backing)
        {
            MethodBuilder get, set;
            DefineAbstractProperty(t, name, propertyType, out get, out set, out backing);
        }

        public static void DefineAbstractProperty(this TypeBuilder t, String name, Type propertyType, bool autoImpl)
        {
            MethodBuilder get, set;
            if (autoImpl)
            {
                FieldBuilder backing;
                DefineAbstractProperty(t, name, propertyType, out get, out set, out backing);
            }
            else
            {
                DefineAbstractProperty(t, name, propertyType, out get, out set);
            }
        }

        public static void DefineAbstractProperty(
            this TypeBuilder t, String name, Type propertyType, out MethodBuilder get, out MethodBuilder set)
        {
            var prop = t.DefineProperty(name, PropertyAttributes.None, propertyType, new Type[0]);
            get = t.DefineMethod("get_" + name, MA.PublicProp | MA.Abstract, propertyType, new Type[0]);
            set = t.DefineMethod("set_" + name, MA.PublicProp | MA.Abstract, null, new[] { propertyType });
            prop.SetGetMethod(get);
            prop.SetSetMethod(set);
        }

        public static void DefineAbstractProperty(
            this TypeBuilder t, String name, Type propertyType, out MethodBuilder get, out MethodBuilder set, out FieldBuilder backing)
        {
            t.DefineAbstractProperty(name, propertyType, out get, out set);

            backing = t.DefineField("_" + name.ToLower(), propertyType, FA.Private);
            get.il().ldarg(0).ldfld(backing).ret();
            set.il().ldarg(0).ldarg(1).stfld(backing).ret();
        }

        public static MethodBuilder DefineAbstractReadonlyProperty(
            this TypeBuilder t, String name, Type propertyType, bool autoImpl)
        {
            if (autoImpl)
            {
                FieldBuilder backing;
                return DefineAbstractReadonlyProperty(t, name, propertyType, out backing);
            }
            else
            {
                return DefineAbstractReadonlyProperty(t, name, propertyType);
            }
        }

        public static MethodBuilder DefineAbstractReadonlyProperty(
            this TypeBuilder t, String name, Type propertyType)
        {
            var vpp = t.DefineProperty(name, PropA.None, propertyType, new Type[0]);
            var get = t.DefineMethod("get_" + name, MA.PublicProp | MA.Abstract, propertyType, new Type[0]);
            vpp.SetGetMethod(get);

            return get;
        }

        public static MethodBuilder DefineAbstractReadonlyProperty(
            this TypeBuilder t, String name, Type propertyType, out FieldBuilder backing)
        {
            var get = t.DefineAbstractReadonlyProperty(name, propertyType);
            backing = t.DefineField("_" + name.ToLower(), propertyType, FA.Private);
            get.il().ldarg(0).ldfld(backing).ret();

            return get;
        }

        public static MethodBuilder DefineAbstractWriteonlyProperty(
            this TypeBuilder t, String name, Type propertyType, bool autoImpl)
        {
            if (autoImpl)
            {
                FieldBuilder backing;
                return DefineAbstractWriteonlyProperty(t, name, propertyType, out backing);
            }
            else
            {
                return DefineAbstractWriteonlyProperty(t, name, propertyType);
            }
        }

        public static MethodBuilder DefineAbstractWriteonlyProperty(
            this TypeBuilder t, String name, Type propertyType)
        {
            var vpp = t.DefineProperty(name, PropA.None, propertyType, new Type[0]);
            var set = t.DefineMethod("set_" + name, MA.PublicProp | MA.Abstract, null, new[] { propertyType });
            vpp.SetSetMethod(set);

            return set;
        }

        public static MethodBuilder DefineAbstractWriteonlyProperty(
            this TypeBuilder t, String name, Type propertyType, out FieldBuilder backing)
        {
            var set = t.DefineAbstractWriteonlyProperty(name, propertyType);
            backing = t.DefineField("_" + name.ToLower(), propertyType, FA.Private);
            set.il().ldarg(0).ldarg(1).stfld(backing).ret();

            return set;
        }

        public static MethodBuilder DefineOverride(this TypeBuilder t, MethodInfo @base)
        {
            MethodAttributes fixt = 0;
            var allAttrs = Enum.GetValues(typeof(MethodAttributes)).Cast<MethodAttributes>();
            var validAttrs = allAttrs
                .Where(a => a != MethodAttributes.Abstract)
                .Where(a => a != MethodAttributes.NewSlot);
            validAttrs.ForEach(a => fixt |= (a & @base.Attributes));

            var @override = t.DefineMethod(
                @base.Name,
                fixt,
                @base.ReturnType,
                @base.Args());

            return @override;
        }

        public static void DefineOverride(
            this TypeBuilder t, PropertyInfo @base, out MethodBuilder get, out MethodBuilder set)
        {
            var prop = t.DefineProperty(@base.Name,
                @base.Attributes,
                @base.PropertyType,
                @base.GetIndexParameters().Select(p => p.ParameterType).ToArray());

            if (@base.CanRead)
            {
                get = t.DefineOverride(@base.GetGetMethod());
                prop.SetGetMethod(get);
            }
            else
            {
                get = null;
            }

            if (@base.CanWrite)
            {
                set = t.DefineOverride(@base.GetSetMethod());
                prop.SetSetMethod(set);
            }
            else
            {
                set = null;
            }
        }

        public static MethodBuilder DefineOverrideReadonly(this TypeBuilder t, PropertyInfo @base)
        {
            (@base.CanRead && !@base.CanWrite).AssertTrue();

            var prop = t.DefineProperty(@base.Name,
                @base.Attributes,
                @base.PropertyType,
                @base.GetIndexParameters().Select(p => p.ParameterType).ToArray());

            var @override = t.DefineOverride(@base.GetGetMethod());
            prop.SetGetMethod(@override);
            return @override;
        }

        public static MethodBuilder DefineOverrideWriteonly(this TypeBuilder t, PropertyInfo @base)
        {
            (!@base.CanRead && @base.CanWrite).AssertTrue();

            var prop = t.DefineProperty(@base.Name,
                @base.Attributes,
                @base.PropertyType,
                @base.GetIndexParameters().Select(p => p.ParameterType).ToArray());

            var @override = t.DefineOverride(@base.GetSetMethod());
            prop.SetSetMethod(@override);
            return @override;
        }
    }
}