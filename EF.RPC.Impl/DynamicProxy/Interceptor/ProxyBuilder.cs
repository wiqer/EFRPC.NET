using EF.RPC.Impl.annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace EF.RPC.Impl.ProducerImpl.Interceptor
{
    /// <summary>
    /// 具体的某个拦截器
    /// </summary>
    public class Interceptor : IInterceptor
    {
        public object Intercept(Invocation invocation)
        {
            Console.WriteLine("煮熟");
            return invocation.Proceed();
        }
    }
    public class Food
    {
        [Rewrite]
        public virtual string Eat(int p1, int p2)
        {
            return "吃";
        }
    }
    public class Test
    {
        public void Run()
        {
            Food f = (Food)Activator.CreateInstance(ProxyBuilder<Interceptor>.BuildProxy(typeof(Food)));
        }
    }

    /// <summary>
    /// 拦截器
    /// </summary>
    public interface IInterceptor
    {
        object Intercept(Invocation invocation);
    }
    /// <summary>
    /// 代理生成类
    /// </summary>
    public class ProxyBuilder<T> where T : IInterceptor, new()
    {
        protected static AssemblyName DemoName = new AssemblyName("DynamicAssembly");
        /// <summary>
        /// 在内存中保存好存放代理类的动态程序集
        // </summary>
        protected static AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(DemoName, AssemblyBuilderAccess.Run);
        /// <summary>
        /// 在内存中保存好存放代理类的托管模块
        /// </summary>
        protected static ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(DemoName.Name);
        /// <summary>
        /// 动态构造targetType的代理类
        /// </summary>
        /// <returns></returns>
        public static Type BuildProxy(Type targetType, bool declaredOnly = false)
        {
            //创建一个类型 
            if (targetType.IsInterface)
            {
                throw new Exception("cannot create a proxy class for the interface");
            }
            Type TypeOfParent = targetType;
            Type[] TypeOfInterfaces = new Type[0];
            TypeBuilder typeBuilder = modBuilder.DefineType(targetType.Name + "Proxy" + Guid.NewGuid().ToString("N"), TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.BeforeFieldInit, TypeOfParent, TypeOfInterfaces);
            BindingFlags bindingFlags;
            if (declaredOnly)
            {
                bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            }
            else
            {
                bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            }
            MethodInfo[] targetMethods = targetType.GetMethods(bindingFlags);
            //遍历各个方法
            foreach (MethodInfo targetMethod in targetMethods)
            {
                //只挑出virtual的实例方法进行重写
                //只挑出打了RewriteAttribute标记的方法进行重写
                if (targetMethod.IsVirtual && !targetMethod.IsStatic && !targetMethod.IsFinal && !targetMethod.IsAssembly && targetMethod.GetCustomAttributes(true).Any(e => (e as RewriteAttribute != null)))
                {
                    Type[] paramType;
                    Type returnType;
                    ParameterInfo[] paramInfo;
                    Type delegateType = GetDelegateType(targetMethod, out paramType, out returnType, out paramInfo);
                    Type interceptorType = typeof(T);
                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(targetMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig, returnType, paramType);
                    for (var i = 0; i < paramInfo.Length; i++)
                    {
                        ParameterBuilder paramBuilder = methodBuilder.DefineParameter(i + 1, paramInfo[i].Attributes, paramInfo[i].Name);
                        if (paramInfo[i].HasDefaultValue)
                        {
                            paramBuilder.SetConstant(paramInfo[i].DefaultValue);
                        }
                    }
                    ILGenerator il = methodBuilder.GetILGenerator();
                    #region 下面的il相当于
                    //下面的il相当于
                    //public class parent
                    //{
                    //    public virtual string test(List<string> p1, int p2)
                    //    {
                    //        return "123";
                    //    }
                    //}
                    //public class child : parent
                    //{
                    //    public override string test(List<string> p1, int p2)
                    //    {
                    //        object[] Parameter = new object[2];
                    //        Parameter[0] = p1;
                    //        Parameter[1] = p2;
                    //        Func<List<string>, int, string> DelegateMethod = base.test;

                    //        Invocation invocation = new Invocation();
                    //        invocation.Parameter = Parameter;
                    //        invocation.DelegateMethod = DelegateMethod;
                    //        Interceptor interceptor = new Interceptor();
                    //        return (string)interceptor.Intercept(invocation);
                    //    }
                    //}
                    #endregion
                    Label label1 = il.DefineLabel();

                    il.DeclareLocal(typeof(object[]));
                    il.DeclareLocal(delegateType);
                    il.DeclareLocal(typeof(Invocation));
                    il.DeclareLocal(interceptorType);
                    LocalBuilder re = null;
                    if (returnType != typeof(void))
                    {
                        re = il.DeclareLocal(returnType);
                    }
                    il.Emit(OpCodes.Ldc_I4, paramType.Length);
                    il.Emit(OpCodes.Newarr, typeof(object));
                    il.Emit(OpCodes.Stloc, 0);
                    for (var i = 0; i < paramType.Length; i++)
                    {
                        il.Emit(OpCodes.Ldloc, 0);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Ldarg, i + 1);
                        if (paramType[i].IsValueType)
                        {
                            il.Emit(OpCodes.Box, paramType[i]);
                        }
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                    il.Emit(OpCodes.Ldarg, 0);
                    il.Emit(OpCodes.Ldftn, targetMethod);
                    il.Emit(OpCodes.Newobj, delegateType.GetConstructors()[0]);
                    il.Emit(OpCodes.Stloc, 1);
                    il.Emit(OpCodes.Newobj, typeof(Invocation).GetConstructors(BindingFlags.Public | BindingFlags.Instance).First(e => e.GetParameters().Length == 0));
                    il.Emit(OpCodes.Stloc, 2);
                    il.Emit(OpCodes.Ldloc, 2);
                    il.Emit(OpCodes.Ldloc, 0);
                    il.Emit(OpCodes.Callvirt, typeof(Invocation).GetMethod("set_Parameter"));
                    il.Emit(OpCodes.Ldloc, 2);
                    il.Emit(OpCodes.Ldloc, 1);
                    il.Emit(OpCodes.Callvirt, typeof(Invocation).GetMethod("set_DelegateMethod"));
                    il.Emit(OpCodes.Newobj, interceptorType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First(e => e.GetParameters().Length == 0));
                    il.Emit(OpCodes.Stloc, 3);
                    il.Emit(OpCodes.Ldloc, 3);
                    il.Emit(OpCodes.Ldloc, 2);
                    il.Emit(OpCodes.Callvirt, interceptorType.GetMethod("Intercept"));
                    if (returnType != typeof(void))
                    {
                        il.Emit(OpCodes.Castclass, returnType);
                        il.Emit(OpCodes.Stloc_S, re);
                        il.Emit(OpCodes.Br_S, label1);
                        il.MarkLabel(label1);
                        il.Emit(OpCodes.Ldloc_S, re);
                    }
                    else
                    {
                        il.Emit(OpCodes.Pop);
                    }
                    il.Emit(OpCodes.Ret);
                }
            }
            //真正创建，并返回
            Type proxyType = typeBuilder.CreateTypeInfo();
            return proxyType;
        }
        /// <summary>
        /// 通过MethodInfo获得其参数类型列表，返回类型，和委托类型
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="paramType"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static Type GetDelegateType(MethodInfo targetMethod, out Type[] paramType, out Type returnType, out ParameterInfo[] paramInfo)
        {
            paramInfo = targetMethod.GetParameters();
            //paramType
            paramType = new Type[paramInfo.Length];
            for (int i = 0; i < paramInfo.Length; i++)
            {
                paramType[i] = paramInfo[i].ParameterType;
            }
            //returnType
            returnType = targetMethod.ReturnType;
            //delegateType
            Type delegateType;
            if (returnType == typeof(void))
            {
                switch (paramType.Length)
                {
                    case 0:
                        delegateType = typeof(Action);
                        break;
                    case 1:
                        delegateType = typeof(Action<>).MakeGenericType(paramType);
                        break;
                    case 2:
                        delegateType = typeof(Action<,>).MakeGenericType(paramType);
                        break;
                    case 3:
                        delegateType = typeof(Action<,,>).MakeGenericType(paramType);
                        break;
                    case 4:
                        delegateType = typeof(Action<,,,>).MakeGenericType(paramType);
                        break;
                    case 5:
                        delegateType = typeof(Action<,,,,>).MakeGenericType(paramType);
                        break;
                    case 6:
                        delegateType = typeof(Action<,,,,,>).MakeGenericType(paramType);
                        break;
                    case 7:
                        delegateType = typeof(Action<,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 8:
                        delegateType = typeof(Action<,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 9:
                        delegateType = typeof(Action<,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 10:
                        delegateType = typeof(Action<,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 11:
                        delegateType = typeof(Action<,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 12:
                        delegateType = typeof(Action<,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 13:
                        delegateType = typeof(Action<,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 14:
                        delegateType = typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 15:
                        delegateType = typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    default:
                        delegateType = typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                }
            }
            else
            {
                Type[] arr = new Type[paramType.Length + 1];
                for (int i = 0; i < paramType.Length; i++)
                {
                    arr[i] = paramType[i];
                }
                arr[paramType.Length] = returnType;
                switch (paramType.Length)
                {
                    case 0:
                        delegateType = typeof(Func<>).MakeGenericType(arr);
                        break;
                    case 1:
                        delegateType = typeof(Func<,>).MakeGenericType(arr);
                        break;
                    case 2:
                        delegateType = typeof(Func<,,>).MakeGenericType(arr);
                        break;
                    case 3:
                        delegateType = typeof(Func<,,,>).MakeGenericType(arr);
                        break;
                    case 4:
                        delegateType = typeof(Func<,,,,>).MakeGenericType(arr);
                        break;
                    case 5:
                        delegateType = typeof(Func<,,,,,>).MakeGenericType(arr);
                        break;
                    case 6:
                        delegateType = typeof(Func<,,,,,,>).MakeGenericType(arr);
                        break;
                    case 7:
                        delegateType = typeof(Func<,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 8:
                        delegateType = typeof(Func<,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 9:
                        delegateType = typeof(Func<,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 10:
                        delegateType = typeof(Func<,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 11:
                        delegateType = typeof(Func<,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 12:
                        delegateType = typeof(Func<,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 13:
                        delegateType = typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 14:
                        delegateType = typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 15:
                        delegateType = typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    default:
                        delegateType = typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                }
            }
            return delegateType;

        }
    }

}
