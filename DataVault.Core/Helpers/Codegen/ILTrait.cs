using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DataVault.Core.Helpers.Codegen
{
    public static class ILTrait
    {
        public static ILGenerator DefineLocal(this ILGenerator il, Type type, out LocalBuilder variableInfo)
        {
            variableInfo = il.DeclareLocal(type);
            return il;
        }

        public static ILGenerator DefineLabel(this ILGenerator il, out Label label)
        {
            label = il.DefineLabel();
            return il;
        }

        public static ILGenerator Apply(this ILGenerator il, Func<ILGenerator, ILGenerator> coder)
        {
            if (coder != null) coder(il);
            return il;
        }

        public static ILGenerator add(this ILGenerator il)
        {
            il.Emit(OpCodes.Add);
            return il;
        }

        public static ILGenerator box(this ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            // the line below forbids usage of .box in expressions creation
            // if (!type.IsValueType) throw new ArgumentException("Value type expected", "type");

            il.Emit(OpCodes.Box, type);
            return il;
        }

        public static ILGenerator br(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Br, label);
            return il;
        }

        public static ILGenerator br_s(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Br_S, label);
            return il;
        }


        public static ILGenerator brfalse(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Brfalse, label);
            return il;
        }

        public static ILGenerator brfalse_s(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Brfalse_S, label);
            return il;
        }

        public static ILGenerator brtrue(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Brtrue, label);
            return il;
        }

        public static ILGenerator brtrue_s(this ILGenerator il, Label label)
        {
            il.Emit(OpCodes.Brtrue_S, label);
            return il;
        }


        public static ILGenerator call(this ILGenerator il, MethodInfo method)
        {
            il.EmitCall(OpCodes.Call, method, null);
            return il;
        }


        public static ILGenerator callvirt(this ILGenerator il, MethodInfo method)
        {
            // WHL-382 - it isn't convenitent to check out whether a method is virtual, thus we're doing it here
            il.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
            return il;
        }

        public static ILGenerator ceq(this ILGenerator il)
        {
            il.Emit(OpCodes.Ceq);
            return il;
        }

        public static ILGenerator castclass(this ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            il.Emit(OpCodes.Castclass, type);
            return il;
        }

        public static ILGenerator constrained(this ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            il.Emit(OpCodes.Constrained, type);
            return il;
        }

        public static ILGenerator convert(this ILGenerator il, Type source, Type destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");

            if (source == destination) return il;

            if (source == typeof(Object) && destination.IsValueType) return il.unbox_any(destination);
            if (source.IsValueType && destination == typeof(Object)) return il.box(destination);

            // if (source.IsAssignableFrom(destination)) return this;
            // --> it doesn't work for int? -> int, cause int is assignable from int?

            var converter = LookUpConverter(source, destination);
            if (converter != null) // not so beauty, but it's enough for internal code
            {
                // todo. implement invariant culture here
                if (converter is ConstructorInfo) return il.newobj((ConstructorInfo) converter);
                // note the ClassCastException expected below in near future :)
                return converter.IsVirtual ? il.callvirt((MethodInfo) converter) : il.call((MethodInfo) converter);
            }

            Func<ILGenerator, ILGenerator> emitter;
            if (CanGenerateConverter(source, destination, out emitter)) return emitter(il);

            return il.castclass(destination);
        }

        #region Coverter stuff

        private static bool CanGenerateConverter(Type source, Type destination, out Func<ILGenerator, ILGenerator> emitter)
        {
            emitter = LookUpForString2Nullable(source, destination) ??
                LookUpForNullable2String(source, destination) ??
                    LookUpForEnum2String(source, destination) ??
                        LookUpForString2Enum(source, destination) ??
                            LookUpForStruct2String(source, destination) ??
                                LookUpForClass2String(source, destination)
                ;
            return emitter != null;
        }

        private static MethodBase LookUpConverter(Type source, Type destination)
        {
            return
                LookUpForOperator(source, destination) ??
                    LookUpForConvertMethod(source, destination) ??
                        LookUpForWrapper(source, destination)
                ;
        }

        private static MethodInfo LookUpForOperator(Type source, Type destination)
        {
            MethodInfo result;

            var ps = MethodAttributes.Public | MethodAttributes.Static;
            if (destination.HasMethod(out result, ps, destination, "op_Explicit", source)) return result;
            if (destination.HasMethod(out result, ps, destination, "op_Implicit", source)) return result;
            if (source.HasMethod(out result, ps, destination, "op_Explicit", source)) return result;
            if (source.HasMethod(out result, ps, destination, "op_Implicit", source)) return result;

            return result;
        }

        private static MethodBase LookUpForWrapper(Type source, Type destination)
        {
            if (destination.IsInterface || destination.IsAbstract) return null;

            // assume wrapper
            var constructor = destination.GetConstructor(new []{source});
            if (constructor != null) return constructor;

            // assume parameterized wrapper
            if (destination.IsGenericType && destination.GetGenericArguments().Length == 1)
            {
                try
                {
                    var def = destination.GetGenericTypeDefinition();
                    var type = def.MakeGenericType(source);
                    constructor = type.GetConstructor(new[] { source });
                }
                catch (ArgumentException)
                {
                    // will be thrown if def.MakeGenericType(source)
                    // violates generic constraints for the type def
                }
            }

            return constructor;
        }

        private static MethodBase LookUpForConvertMethod(Type source, Type destination)
        {
            MethodInfo converter = null;
            // Convert.Toxxx(yyy);
            if (
                (source == typeof(String) && destination.IsPrimitive) ||
                    (source.IsPrimitive && destination == typeof(String)) ||
                        (source == typeof(String) && destination == typeof(DateTime)) ||
                            (source == typeof(DateTime) && destination == typeof(String)) ||
                                (source.IsPrimitive && destination == typeof(DateTime)) ||
                                    (source == typeof(DateTime) && destination.IsPrimitive) ||
                                        (source.IsPrimitive && destination.IsPrimitive)
                )
            {
                converter = typeof(Convert).GetMethod("To" + destination.Name, new Type[] {source});
                if (converter == null) throw new ArgumentException(String.Format("There is no converter from {0} to {1}", source.Name, destination.Name), "destination");
            }
            return converter;
        }

        private static Func<ILGenerator, ILGenerator> LookUpForNullable2String(Type source, Type destination)
        {
            if (!(source.IsGenericType && source.GetGenericTypeDefinition() == typeof(Nullable<>))) return null;
            if (typeof(String) != destination) return null;

            LocalBuilder primitive;
            Label @else;
            Label endIf;

            // nullable<?> is on top of the stack
            return il => il
                .DefineLocal(source, out primitive)
                .DefineLabel(out @else)
                .DefineLabel(out endIf)
                .stloc(primitive)
                .ldloca(primitive)
                .call(source.GetProperty("HasValue").GetGetMethod())
                .brfalse_s(@else) // @if true
                .ldloca(primitive)
                .constrained(source)
                .callvirt(typeof(Object).GetMethod("ToString"))
                .br_s(endIf)
                .label(@else) // @else
                .ldnull()
                .label(endIf) // @end
                ;
        }

        private static Func<ILGenerator, ILGenerator> LookUpForString2Nullable(Type source, Type destination)
        {
            if (typeof(String) != source) return null;
            if (!(destination.IsGenericType && destination.GetGenericTypeDefinition() == typeof(Nullable<>))) return null;

            var valueType = destination.GetGenericArguments()[0];
            //if (!valueType.IsPrimitive) return null; // maybe replace this with TryParse 4 all structs?

            MethodInfo tryParse;
            if (valueType.HasMethod(out tryParse, MethodAttributes.Static | MethodAttributes.Public,
                typeof(bool), "TryParse", typeof(String), valueType.MakeByRefType()))
            {
                LocalBuilder result;
                LocalBuilder value;
                Label @else;
                Label endIf;

                // String is on top of the stack
                return il => il
                    .DefineLocal(destination, out result)
                    .DefineLocal(valueType, out value)
                    .DefineLabel(out @else)
                    .DefineLabel(out endIf)
                    .ldloca(value)
                    .call(tryParse)
                    .brfalse_s(@else) // @if true
                    .ldloc(value)
                    .newobj(destination, valueType)
                    .stloc(result)
                    .br_s(endIf)
                    .label(@else) // @else
                    .ldloca(result)
                    .initobj(destination)
                    .label(endIf) // @end
                    .ldloc(result)
                    ;
            }

            // finally try .ctor(String)
            var ctor = valueType.GetConstructor(new[] {typeof(String)});
            if (ctor != null)
            {
                LocalBuilder result;
                Label @else;
                Label endIf;
                // String is on top of the stack
                return il => il
                    .DefineLocal(destination, out result)
                    .DefineLabel(out @else)
                    .DefineLabel(out endIf)
                    .dup()
                    // BR: empty strings considered bad initializer argument
                    .call(typeof(String).GetMethod("IsNullOrEmpty"))
                    .brtrue_s(@else) // @if !IsNullOrEmpty
                    .newobj(ctor)
                    .newobj(destination, valueType)
                    .stloc(result)
                    .br_s(endIf)
                    .label(@else) // @else
                    .pop()
                    .ldloca(result)
                    .initobj(destination)
                    .label(endIf) // @end
                    .ldloc(result)
                    ;
            }

            return null;
        }

        private static Func<ILGenerator, ILGenerator> LookUpForEnum2String(Type source, Type destination)
        {
            if (!source.IsEnum) return null;
            if (typeof(String) != destination) return null;

            // unboxed enum is on top of the stack
            return il => il
                .box(source)
                .ldstr("d")
                .call(FromLambda.Method<Enum, String, String>((e, f) => e.ToString(f)))
                ;
        }

        private static Func<ILGenerator, ILGenerator> LookUpForString2Enum(Type source, Type destination)
        {
            if (typeof(String) != source) return null;
            if (!destination.IsEnum) return null;

            LocalBuilder value;

            // String is on top of the stack
            return il => il
                .DefineLocal(source, out value)
                .stloc(value)
                .ld_type_info(destination)
                .ldloc(value)
                .convert(source, Enum.GetUnderlyingType(destination))
                .call(typeof(Enum).GetMethod("ToObject", new[] {typeof(Type), Enum.GetUnderlyingType(destination)}))
                .unbox_any(destination)
                ;
        }

        private static Func<ILGenerator, ILGenerator> LookUpForStruct2String(Type source, Type destination)
        {
            if (!source.IsValueType) return null;
            if (typeof(String) != destination) return null;

            LocalBuilder value;

            // struct is on top of the stack
            return il => il
                .DefineLocal(source, out value)
                .stloc(value)
                .ldloca(value)
                .constrained(source)
                .callvirt(FromLambda.Method<Object, String>(o => o.ToString()))
                ;
        }

        private static Func<ILGenerator, ILGenerator> LookUpForClass2String(Type source, Type destination)
        {
            if (source.IsValueType) return null;
            if (typeof(String) != destination) return null;

            // instance or null is on top of the stack
            return il => il
                .dup()
                .@if(true, x => x
                    .callvirt(FromLambda.Method<Object, String>(o => o.ToString()))
                );
        }

        #endregion

        public static ILGenerator dup(this ILGenerator il)
        {
            il.Emit(OpCodes.Dup);
            return il;
        }

        public static ILGenerator @if(this ILGenerator il, bool condition, Func<ILGenerator, ILGenerator> @true)
        {
            var endBlock = il.DefineLabel();

            // it's possible to calculate length of the "@true(il)" block by visiting delegate body
            // but i'm too lazy to do that :) thus short jumps aren't used
            il.Emit(condition ? OpCodes.Brfalse : OpCodes.Brtrue, endBlock);
            @true(il);
            il.MarkLabel(endBlock);

            return il;
        }

        public static ILGenerator @if(this ILGenerator il, bool condition, Func<ILGenerator, ILGenerator> @true, Func<ILGenerator, ILGenerator> @false)
        {
            var elseMarker = il.DefineLabel();
            var endifMarker = il.DefineLabel();

            il.Emit(condition ? OpCodes.Brfalse : OpCodes.Brtrue, elseMarker);
            @true(il);
            il.Emit(OpCodes.Br, endifMarker);
            il.MarkLabel(elseMarker);
            @false(il);
            il.MarkLabel(endifMarker);

            return il;
        }

        public static ILGenerator initobj(this ILGenerator il, Type valueType)
        {
            il.Emit(OpCodes.Initobj, valueType);
            return il;
        }

        public static ILGenerator isinst(this ILGenerator il, Type valueType)
        {
            il.Emit(OpCodes.Isinst, valueType);
            return il;
        }

        public static ILGenerator label(this ILGenerator il, Label label)
        {
            il.MarkLabel(label);
            return il;
        }

        public static ILGenerator ld_args(this ILGenerator il, int count)
        {
            for (var i = 0; i < count; i++)
                ldarg(il, i);
            return il;
        }

        public static ILGenerator ld_method_info(this ILGenerator il, MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");

            il.Emit(OpCodes.Ldtoken, methodInfo);
            il.EmitCall(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] {typeof(RuntimeMethodHandle)}), null);
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));

            return il;
        }

        public static ILGenerator ld_type_info(this ILGenerator il, Type typeInfo)
        {
            if (typeInfo == null) throw new ArgumentNullException("typeInfo");

            il.Emit(OpCodes.Ldtoken, typeInfo);
            il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] {typeof(RuntimeTypeHandle)}), null);

            return il;
        }

        public static ILGenerator ldarg(this ILGenerator il, int index)
        {
            if (index < 4)
                switch (index)
                {
                    case 0:
                        il.Emit(OpCodes.Ldarg_0);
                        return il;
                    case 1:
                        il.Emit(OpCodes.Ldarg_1);
                        return il;
                    case 2:
                        il.Emit(OpCodes.Ldarg_2);
                        return il;
                    case 3:
                        il.Emit(OpCodes.Ldarg_3);
                        return il;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Index should not be negative");
                }

            if (index > byte.MaxValue) il.Emit(OpCodes.Ldarg, index);
            else il.Emit(OpCodes.Ldarg_S, (byte) index);

            return il;
        }

        public static ILGenerator ldftn(this ILGenerator il, MethodInfo mi)
        {
            il.Emit(OpCodes.Ldftn, mi);
            return il;
        }

        public static ILGenerator ldtrue(this ILGenerator il)
        {
            return il.ldc_i4(1);
        }

        public static ILGenerator ldfalse(this ILGenerator il)
        {
            return il.ldc_i4(0);
        }

        public static ILGenerator ldc_i4(this ILGenerator il, int constant)
        {
            if (constant < 9)
                if (constant > -2)
                    switch (constant)
                    {
                        case 0:
                            il.Emit(OpCodes.Ldc_I4_0);
                            return il;
                        case 1:
                            il.Emit(OpCodes.Ldc_I4_1);
                            return il;
                        case 2:
                            il.Emit(OpCodes.Ldc_I4_2);
                            return il;
                        case 3:
                            il.Emit(OpCodes.Ldc_I4_3);
                            return il;
                        case 4:
                            il.Emit(OpCodes.Ldc_I4_4);
                            return il;
                        case 5:
                            il.Emit(OpCodes.Ldc_I4_5);
                            return il;
                        case 6:
                            il.Emit(OpCodes.Ldc_I4_6);
                            return il;
                        case 7:
                            il.Emit(OpCodes.Ldc_I4_7);
                            return il;
                        case 8:
                            il.Emit(OpCodes.Ldc_I4_8);
                            return il;
                        case -1:
                            il.Emit(OpCodes.Ldc_I4_M1);
                            return il;
                    }
                else
                {
                    il.Emit(OpCodes.Ldc_I4, constant);
                    return il;
                }

            if (constant > sbyte.MaxValue || constant < sbyte.MinValue) il.Emit(OpCodes.Ldc_I4, constant);
            else il.Emit(OpCodes.Ldc_I4_S, (sbyte) constant);

            return il;
        }

        public static ILGenerator ldc_i8(this ILGenerator il, long constant)
        {
            il.Emit(OpCodes.Ldc_I8, constant);
            return il;
        }

        public static ILGenerator ldc_r4(this ILGenerator il, float constant)
        {
            il.Emit(OpCodes.Ldc_R4, constant);
            return il;
        }

        public static ILGenerator ldc_r8(this ILGenerator il, double constant)
        {
            il.Emit(OpCodes.Ldc_R8, constant);
            return il;
        }

        public static ILGenerator lddefault(this ILGenerator il, Type type)
        {
            if (type == typeof(void)) return il; // usualy used in props autogen

            if (type.IsPrimitive)
            {
                if (typeof(bool) == type || typeof(byte) == type || typeof(sbyte) == type || typeof(short) == type ||
                    typeof(ushort) == type || typeof(int) == type || typeof(uint) == type || typeof(char) == type)
                    return il.ldc_i4(0);

                if (typeof(float) == type) return il.ldc_r4(0);
                if (typeof(double) == type) return il.ldc_r8(0.0);

                if (typeof(long) == type || typeof(ulong) == type) return il.ldc_i8(0);

                throw new ArgumentOutOfRangeException("type", "Unexpected primitive type: " + type);
            }

            if (type.IsValueType)
            {
                var variable = il.DeclareLocal(type);
                return il.ldloca(variable).initobj(type).ldloc(variable);
            }

            return il.ldnull();
        }

        public static ILGenerator ldfld(this ILGenerator il, FieldInfo field)
        {
            il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
            return il;
        }

        public static ILGenerator ldloc(this ILGenerator il, LocalVariableInfo variable)
        {
            if (variable.LocalIndex < 4)
                switch (variable.LocalIndex)
                {
                    case (0):
                        il.Emit(OpCodes.Ldloc_0);
                        return il;
                    case (1):
                        il.Emit(OpCodes.Ldloc_1);
                        return il;
                    case (2):
                        il.Emit(OpCodes.Ldloc_2);
                        return il;
                    case (3):
                        il.Emit(OpCodes.Ldloc_3);
                        return il;
                    default:
                        throw new ArgumentOutOfRangeException("variable", "Variable index should be positive");
                }

            if (variable.LocalIndex > byte.MaxValue)
                il.Emit(OpCodes.Ldloc, variable.LocalIndex);
            else
                il.Emit(OpCodes.Ldloc_S, (byte) variable.LocalIndex);
            return il;
        }

        public static ILGenerator ldloca(this ILGenerator il, LocalVariableInfo variable)
        {
            if (variable.LocalIndex > byte.MaxValue)
                il.Emit(OpCodes.Ldloca, variable.LocalIndex);
            else
                il.Emit(OpCodes.Ldloca_S, (byte) variable.LocalIndex);
            return il;
        }

        public static ILGenerator ldnull(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldnull);
            return il;
        }

        public static ILGenerator ldtoken(this ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            il.Emit(OpCodes.Ldtoken, type);
            return il;
        }

        public static ILGenerator ldtoken(this ILGenerator il, MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");

            il.Emit(OpCodes.Ldtoken, methodInfo);
            return il;
        }

        public static ILGenerator ldstr(this ILGenerator il, String constant)
        {
            if (constant == null) return il.ldnull();

            il.Emit(OpCodes.Ldstr, constant);
            return il;
        }

        public static ILGenerator newarr(this ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            il.Emit(OpCodes.Newarr, type);
            return il;
        }

        public static ILGenerator newobj(this ILGenerator il, ConstructorInfo ctor)
        {
            if (ctor == null) throw new ArgumentNullException("ctor");

            il.Emit(OpCodes.Newobj, ctor);
            return il;
        }

        public static ILGenerator newobj(this ILGenerator il, Type type, params Type[] ctorParams)
        {
            if (type == null) throw new ArgumentNullException("type");

            var ctor = type.GetConstructor(ctorParams);
            if (ctor != null) return il.newobj(ctor);

            ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Standard, ctorParams, null);
            if (ctor != null) return il.newobj(ctor);

            throw new ArgumentException(String.Format("No such .ctor({1}) for type {0}.", type, ctorParams));
        }


        public static ILGenerator pop(this ILGenerator il)
        {
            il.Emit(OpCodes.Pop);
            return il;
        }

        public static ILGenerator ret(this ILGenerator il)
        {
            il.Emit(OpCodes.Ret);
            return il;
        }

        public static ILGenerator stelem_ref(this ILGenerator il)
        {
            il.Emit(OpCodes.Stelem_Ref);
            return il;
        }

        public static ILGenerator stfld(this ILGenerator il, FieldInfo field)
        {
            il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
            return il;
        }

        public static ILGenerator stloc(this ILGenerator il, LocalBuilder variable)
        {
            if (variable == null) return il; // do nothing for void

            if (variable.LocalIndex < 4)
            {
                switch (variable.LocalIndex)
                {
                    case 0:
                        il.Emit(OpCodes.Stloc_0);
                        return il;
                    case 1:
                        il.Emit(OpCodes.Stloc_1);
                        return il;
                    case 2:
                        il.Emit(OpCodes.Stloc_2);
                        return il;
                    case 3:
                        il.Emit(OpCodes.Stloc_3);
                        return il;
                    default:
                        throw new NotImplementedException("An unexpected place :(");
                }
            }

            if (variable.LocalIndex > byte.MaxValue)
                il.Emit(OpCodes.Stloc, variable);
            else
                il.Emit(OpCodes.Stloc_S, variable);

            return il;
        }

        public static ILGenerator @throw(this ILGenerator il)
        {
            il.Emit(OpCodes.Throw);
            return il;
        }

        public static ILGenerator @throw(this ILGenerator il, Type exceptionType, params Type[] ctorParamTypes)
        {
            if (exceptionType == null) throw new ArgumentNullException("exceptionType");
            if (!typeof(Exception).IsAssignableFrom(exceptionType)) throw new ArgumentException("Exception type expected.", "exceptionType");

            var constructor = exceptionType.GetConstructor(ctorParamTypes);
            if (constructor == null) throw new ArgumentNullException("exceptionType", "No ctor found");

            return il.newobj(constructor).@throw();
        }

        public static ILGenerator unbox(this ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!type.IsValueType) throw new ArgumentException("Value type expected", "type");

            il.Emit(OpCodes.Unbox, type);
            return il;
        }

        public static ILGenerator unbox_any(this ILGenerator il, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            il.Emit(OpCodes.Unbox_Any, type);
            return il;
        }

        public static ILGenerator xor(this ILGenerator il)
        {
            il.Emit(OpCodes.Xor);
            return il;
        }
    }
}