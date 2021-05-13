using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace EF.RPC.Impl.ProducerImpl
{

    /// <summary>
    /// 代理类
    /// </summary>
    public static class DynamicProxyFactory
    {
        private static readonly string AssemblyName = "DynamicProxyAssembly";
        private static readonly string ModuleName = "DynamicProxyModule";
        private static readonly string TypeName = "DynamicProxy";

        /// <summary>
        /// 因为有些方法的指令是需要拆箱装箱的
        /// </summary>
        private static readonly HashSet<Type> CanBox = new HashSet<Type>
    {
        typeof(int), typeof(uint),
        typeof(short), typeof(ushort),
        typeof(long), typeof(ulong),
        typeof(float), typeof(double),
        typeof(sbyte), typeof(byte),
        typeof(char),
        typeof(decimal),
    };

        private static readonly Dictionary<Type, ProxyTypeInfo> ProxyDict = new Dictionary<Type, ProxyTypeInfo>();

        private static TypeBuilder createDynamicTypeBuilder(Type type, Type parent, Type[] interfaces)
        {
            if (ProxyDict.TryGetValue(type, out var info))
            {
                info.Count++;
            }
            else
            {
                ProxyDict[type] = info = new ProxyTypeInfo
                {
                    Count = 1
                };
            }

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName + type.Name),
                AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(ModuleName + type.Name);
            return info.TypeBuilder = moduleBuilder.DefineType(TypeName + type.Name + info.Count,
                TypeAttributes.Public | TypeAttributes.Class, parent, interfaces);
        }
        /// <summary>
        /// 函数作用：
        ///  public class II : InvocationHandlerInterface
        //{
        //    public object invoke(object proxy, MethodInfo method, object[] args)
        //    {
        //        foreach (object o in args)
        //        {
        //            Console.WriteLine(o.ToString());
        //        }
        //        Console.WriteLine($"hahahaha");
        //        return args[1];
        //    }
        //}
        //public interface myInvocationHandlerInterface
        //{
        //    object m(string args, string arg2);
        //    object kk(string args, string arg2);
        //}
        //proxyInit生成的代理类模板
        //public class DynamicProxy1
        //{
        //public DynamicProxy1(II _handler, MethodInfo _methodInfos) {
        //    this._handler = _handler;
        //    this._methodInfos = _methodInfos;
        //}
        //    private II _handler;
        //    private MethodInfo _methodInfos;
        //    object m(string args, string arg2)
        //    {

        //        return _handler.invoke(_handler, _methodInfos, new object[] { args, arg2 });
        //    }
        //    object kk(string args, string arg2)
        //    {
        //        return _handler.invoke(_handler, _methodInfos, new object[] { args, arg2 });
        //    }

        //}
        /// </summary>
        private static void proxyInit(Type type, TypeBuilder typeBuilder, MethodInfo[] methodInfos,
            MethodInfo handlerInvokeMethodInfo)
        {
            //定义两个字段
            var handlerFieldBuilder =
                typeBuilder.DefineField("_handler", typeof(InvocationHandlerInterface), FieldAttributes.Private);
            var methodInfosFieldBuilder =
                typeBuilder.DefineField("_methodInfos", typeof(MethodInfo), FieldAttributes.Private);
            //定义构造函数
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new[] { typeof(InvocationHandlerInterface), typeof(MethodInfo[]) });
            var ilCtor = constructorBuilder.GetILGenerator();
            ilCtor.Emit(OpCodes.Ldarg_0);
            ilCtor.Emit(OpCodes.Call,
                typeof(object).GetConstructor(new Type[0]) ?? throw new Exception("不可能的错误:object.GetConstructor"));
            ilCtor.Emit(OpCodes.Ldarg_0);
            ilCtor.Emit(OpCodes.Ldarg_1);
            ilCtor.Emit(OpCodes.Stfld, handlerFieldBuilder);
            ilCtor.Emit(OpCodes.Ldarg_0);
            ilCtor.Emit(OpCodes.Ldarg_2);
            ilCtor.Emit(OpCodes.Stfld, methodInfosFieldBuilder);
            ilCtor.Emit(OpCodes.Ret);

            for (var i = 0; i < methodInfos.Length; i++)
            {
                var methodInfo = methodInfos[i];
                var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    methodInfo.CallingConvention, methodInfo.ReturnType, parameterTypes);
                var ilMethod = methodBuilder.GetILGenerator();
                ilMethod.Emit(OpCodes.Ldarg_0);
                ilMethod.Emit(OpCodes.Ldfld, handlerFieldBuilder);
                ilMethod.Emit(OpCodes.Ldarg_0);
                ilMethod.Emit(OpCodes.Ldarg_0);
                ilMethod.Emit(OpCodes.Ldfld, methodInfosFieldBuilder);
                ilMethod.Emit(OpCodes.Ldc_I4, i);
                ilMethod.Emit(OpCodes.Ldelem_Ref);
                ilMethod.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
                ilMethod.Emit(OpCodes.Newarr, typeof(object));
                for (var j = 0; j < parameterTypes.Length; j++)
                {
                    ilMethod.Emit(OpCodes.Dup);
                    ilMethod.Emit(OpCodes.Ldc_I4_S, (short)j);
                    ilMethod.Emit(OpCodes.Ldarg_S, (short)(j + 1));
                    if (CanBox.Contains(parameterTypes[j]))
                    {
                        ilMethod.Emit(OpCodes.Box, parameterTypes[j]);
                    }

                    ilMethod.Emit(OpCodes.Stelem_Ref);
                }

                ilMethod.Emit(OpCodes.Callvirt, handlerInvokeMethodInfo);
                ilMethod.Emit(CanBox.Contains(methodInfo.ReturnType) ? OpCodes.Unbox_Any : OpCodes.Castclass,
                    methodInfo.ReturnType);
                ilMethod.Emit(OpCodes.Ret);
            }
        }

        /// <summary>
        /// 通过接口创建动态代理
        /// </summary>
        public static T createProxyByInterface<T>(InvocationHandlerInterface handler, bool userCache = true)
        {
            return (T)createProxyByInterface(typeof(T), handler, userCache);
        }

        public static object createProxyByInterface(Type type, InvocationHandlerInterface handler, bool userCache = true)
        {
            if (!userCache || !ProxyDict.TryGetValue(type, out var info))
            {
                var handlerInvokeMethodInfo = typeof(InvocationHandlerInterface).GetMethod("invoke") ??
                                              throw new Exception("不可能的错误:handlerInvokeMethodInfo");
                var typeBuilder = createDynamicTypeBuilder(type, null, new[] { type });
                var methodInfos = type.GetMethods();
                proxyInit(type, typeBuilder, methodInfos, handlerInvokeMethodInfo);
                info = ProxyDict[type];
                if (info.Count == 1)
                {
                    info.MethodInfos = methodInfos;
                }
            }
            //Type t = info.TypeBuilder.CreateTypeInfo();
            return Activator.CreateInstance(info.TypeBuilder.CreateTypeInfo(), handler, info.MethodInfos) ??
                                              throw new Exception("不同环境此处可能需要改写");
        }
        /// <summary>
        /// 通过类创建动态代理
        /// </summary>
        public static T createProxyByType<T>(InvocationHandlerInterface handler, bool userCache = true)
        {
            return (T)createProxyByType(typeof(T), handler, userCache);
        }

        public static object createProxyByType(Type type, InvocationHandlerInterface handler, bool userCache = true)
        {
            if (!userCache || !ProxyDict.TryGetValue(type, out var info))
            {
                var handlerInvokeMethodInfo = typeof(InvocationHandlerInterface).GetMethod("invoke") ??
                                              throw new Exception("不可能的错误:handlerinvokeMethodInfo");
                var typeBuilder = createDynamicTypeBuilder(type, type, null);
                var methodInfos = type.GetMethods().Where(methodInfo => methodInfo.IsVirtual || methodInfo.IsAbstract)
                    .ToArray();
                proxyInit(type, typeBuilder, methodInfos, handlerInvokeMethodInfo);
                info = ProxyDict[type];
                if (info.Count == 1)
                {
                    info.MethodInfos = methodInfos;
                }
            }

            return Activator.CreateInstance(info.TypeBuilder.CreateTypeInfo(), handler, info.MethodInfos) ??
                                              throw new Exception("不同环境此处可能需要改写");
        }
        /// <summary>
        /// 缓存类
        /// </summary>
        class ProxyTypeInfo
        {
            public TypeBuilder TypeBuilder;
            public int Count;
            public MethodInfo[] MethodInfos;
        }
    }

    /// <summary>
    /// 示例
    /// </summary>
    public class II : InvocationHandlerInterface
    {
        public object invoke(object proxy, MethodInfo method, object[] args)
        {
            foreach (object o in args)
            {
                Console.WriteLine(o.ToString());
            }
            Console.WriteLine($"hahahaha");
            return args[1];
        }
    }

    /// <summary>
    /// 示例
    /// </summary>
    public interface myInvocationHandlerInterface
    {
        object m(string args, string arg2);
        object kk(string args, string arg2);
    }
    //public class DynamicProxy1 {
    //    private II _handler;
    //    private MethodInfo _methodInfos;
    //    object m(string args, string arg2) {

    //        return _handler.invoke(_handler, _methodInfos, new object[] { args, arg2 });
    //    }
    //    object kk(string args, string arg2) {
    //        return _handler.invoke(_handler, _methodInfos, new object[] { args, arg2 });
    //    }

    //}

}
