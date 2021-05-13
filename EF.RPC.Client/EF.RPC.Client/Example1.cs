using System;
using System.Reflection;
using System.Reflection.Emit;
namespace EF.RPC.Client
{
    public interface IConsole
    {
        void Say();
        void Cry();
        void falg(string s);
        ExampleRep mmp(ExampleReq req);
    }
    public class ExampleIMPL
    {
       public static void  Cry1(string d) {
            Console.WriteLine(d);
            Console.WriteLine($"hahahaha");
        }
        public static void Cry2()
        {
           
            Console.WriteLine($"hahahaha");
        }
        public static void Cry3()
        {

            Console.WriteLine($"hahahaha");
        }
        public ExampleRep mmp(ExampleReq req) {
            ExampleRep rep = new ExampleRep();
            rep.sum = req.Num1 + req.Num2;
            return rep;
        }
        public static void falg(string s) {
            Console.WriteLine(s);
        }
    }
    public class ExampleReq
    {
        public ExampleReq() {
            Num1 = Num2 = 6;
        }
        public int Num1 { get; set; }

        public int Num2 { get; set; }
    }
    public class ExampleRep
    {
        public int sum { get; set; }

     
    }
    class Example1
    {
        public static void Run()
        {
            // 在看下面的代码之前，先明白这个依赖关系，即：
            // 方法->类型->模块->程序集

            MethodInfo[] methodInfos = typeof(ExampleIMPL).GetMethods();
            //定义程序集的名称
            AssemblyName aName = new AssemblyName("DynamicAssemblyExample");

            // 创建一个程序集构建器
            // Framework应该这样：AppDomain.CurrentDomain.DefineDynamicAssembly
            AssemblyBuilder ab =
                AssemblyBuilder.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.Run);


            // 使用程序集构建器创建一个模块构建器
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name + ".dll");

            // 使用模块构建器创建一个类型构建器
            TypeBuilder tb = mb.DefineType("DynamicConsole");

            // 使类型实现IConsole接口
            tb.AddInterfaceImplementation(typeof(IConsole));

            var attrs = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Final;

            // 使用类型构建器创建一个方法构建器
            MethodBuilder methodBuilder = tb.DefineMethod("Say", attrs, typeof(void), Type.EmptyTypes);
            MethodBuilder methodBuilder2 = tb.DefineMethod("Cry", attrs, typeof(void), Type.EmptyTypes);
            MethodBuilder methodBuilder4 = tb.DefineMethod("falg", attrs, typeof(void), new[] { typeof(string) });
            MethodBuilder methodBuilder3;// = tb.DefineMethod("Cry", attrs, typeof(void), Type.EmptyTypes);
            MethodInfo[] WriteLinemethodInfoss = typeof(IConsole).GetMethods();
            foreach (MethodInfo  m in WriteLinemethodInfoss) {
                if (m.Name.Equals("mmp")) {
                    ParameterInfo[]  ty=m.GetParameters();
                    Type[] types=new Type[ty.Length];
                    for (int ii= 0;ii<ty.Length ;ii++)
                    {
                        types[ii] = ty[ii].ParameterType;
                    }
                       
                    methodBuilder3 = tb.DefineMethod("mmp", attrs, m.ReturnType, types);
                    ILGenerator IL3 = methodBuilder3.GetILGenerator();
                    MethodInfo methodInfo3 = typeof(ExampleIMPL).GetMethod("mmp", types);
                    IL3.Emit(OpCodes.Ldstr, "I'm here.");
                    IL3.EmitCall(OpCodes.Call, methodInfo3, types);

                    // 退出函数
                    IL3.Emit(OpCodes.Ret);
                }
            }
            //BindingFlags bindingFlags = BindingFlags.Public;
            //methodBuilder2.Invoke(Activator.CreateInstance(typeof(ExampleIMPL)), null);
            // 通过方法构建器获取一个MSIL生成器
            ILGenerator IL2 = methodBuilder2.GetILGenerator();
            MethodInfo methodInfo = typeof(ExampleIMPL).GetMethod("Cry1", new[] { typeof(string) });
        


            IL2.Emit(OpCodes.Ldstr, "I'm here.");
      
            // 调用Console.Writeline函数
            IL2.Emit(OpCodes.Call, methodInfo);

            // 退出函数
            IL2.Emit(OpCodes.Ret);

            ILGenerator IL4 = methodBuilder4.GetILGenerator();
            MethodInfo methodInfo4 = typeof(ExampleIMPL).GetMethod("falg", new[] { typeof(string) });



            //IL4.Emit(OpCodes.Ldstr, "I'm here?????????.");
            //IL4.Emit(OpCodes.Ldc_I4, 1);
            //IL4.Emit(OpCodes.Ldc_I4, 0);
            //IL.Emit(OpCodes.Ldc_I4, 3);
            var s=IL4.DeclareLocal(typeof(string));
           
            IL4.Emit(OpCodes.Ldc_I4, 1);
            IL4.Emit(OpCodes.Newarr, typeof(string));
            IL4.Emit(OpCodes.Ldloc, 0);
            //IL.Emit(OpCodes.Ldstr, "土豆");
            IL4.Emit(OpCodes.Stelem, typeof(string));         
            IL4.Emit(OpCodes.Stloc,0);
            IL4.Emit(OpCodes.Ldloc, s);
            // IL4.Emit(OpCodes.Stloc_0);

            // 调用Console.Writeline函数
            IL4.Emit(OpCodes.Call, methodInfo4);

            // 退出函数
            IL4.Emit(OpCodes.Ret);
            //IL2.
            // 通过方法构建器获取一个MSIL生成器//生成 Microsoft 中间语言 (MSIL) 指令
            ILGenerator IL = methodBuilder.GetILGenerator();

            // 开始编写方法的执行逻辑

            // 将一个字符串压入栈顶
            IL.Emit(OpCodes.Ldstr, "I'm here.");
            MethodInfo WriteLinemethodInfo = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
            // 调用Console.Writeline函数
            IL.Emit(OpCodes.Call, WriteLinemethodInfo);

            // 退出函数
            IL.Emit(OpCodes.Ret);

            //方法结束


            // 从类型构建器中创建出类型
            var dynamicType = tb.CreateType();


            // 通过反射创建出动态类型的实例
            var console = Activator.CreateInstance(dynamicType) as IConsole;

            console.Say();
            console.Cry();
           // console.mmp( new ExampleReq());
            console.falg("kkkkkkk");
            Console.ReadLine();
        }
    }

    class Example2
    {
        public static void Run()
        {
            AssemblyName aName = new AssemblyName("ChefDynamicAssembly");

            AssemblyBuilder ab =
                AssemblyBuilder.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.Run);

            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name + ".dll");

            TypeBuilder tb = mb.DefineType("Commander");

            var attrs = MethodAttributes.Public;

            // 使用类型构建器创建一个方法构建器
            MethodBuilder methodBuilder = tb.DefineMethod("Do", attrs, typeof(string), Type.EmptyTypes);

            // 通过方法构建器获取一个MSIL生成器
            var IL = methodBuilder.GetILGenerator();

            // 开始编写方法的执行逻辑

            // var vegetables = new string[3];
            var vegetables = IL.DeclareLocal(typeof(string[]));
            IL.Emit(OpCodes.Ldc_I4, 3);
            IL.Emit(OpCodes.Newarr, typeof(string));
            IL.Emit(OpCodes.Stloc, vegetables);

            //vegetables[0] = "土豆"
            IL.Emit(OpCodes.Ldloc, vegetables);
            IL.Emit(OpCodes.Ldc_I4, 0);
            IL.Emit(OpCodes.Ldstr, "土豆");
            IL.Emit(OpCodes.Stelem, typeof(string));

            //vegetables[1] = "青椒"
            IL.Emit(OpCodes.Ldloc, vegetables);
            IL.Emit(OpCodes.Ldc_I4, 1);
            IL.Emit(OpCodes.Ldstr, "青椒");
            IL.Emit(OpCodes.Stelem, typeof(string));

            //vegetables[2] = "木耳"
            IL.Emit(OpCodes.Ldloc, vegetables);
            IL.Emit(OpCodes.Ldc_I4, 2);
            IL.Emit(OpCodes.Ldstr, "木耳");
            IL.Emit(OpCodes.Stelem, typeof(string));

            // IChef chef = new GoodChef();
            var chef = IL.DeclareLocal(typeof(IChef));
            IL.Emit(OpCodes.Newobj, typeof(GoodChef).GetConstructor(Type.EmptyTypes));
            IL.Emit(OpCodes.Stloc, chef);

            //var dish = chef.Cook(vegetables);
            var dish = IL.DeclareLocal(typeof(string));
            IL.Emit(OpCodes.Ldloc, chef);
            IL.Emit(OpCodes.Ldloc, vegetables);
            IL.Emit(OpCodes.Callvirt, typeof(IChef).GetMethod("Cook"));
            IL.Emit(OpCodes.Stloc, dish);

            // return dish;
            IL.Emit(OpCodes.Ldloc, dish);
            IL.Emit(OpCodes.Ret);

            //方法结束


            // 从类型构建器中创建出类型
            var dynamicType = tb.CreateType();


            // 通过反射创建出动态类型的实例
            var commander = Activator.CreateInstance(dynamicType);

            Console.WriteLine(dynamicType.GetMethod("Do").Invoke(commander, null).ToString());

            Console.ReadLine();
        }
    }

    public interface IChef
    {
        string Cook(string[] vegetables);
    }

    public class GoodChef : IChef
    {
        public string Cook(string[] vegetables)
        {
            return "good:" + string.Join("+", vegetables);
        }
    }

}
