using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using CSL.SQL;
using System.Threading.Tasks;


namespace CommonStructureLibraryTester
{
    public static partial class Tests
    {
        private record SimpleTest(string Name, Func<bool> Test, int Priority);
        #region Init
        public static void RunTests()
        {
            int passcount = 0;
            int failedcount = 0;
            int testcount = TestList.Count;
            foreach(SimpleTest Test in TestList.OrderByDescending((x)=> x.Priority))
            {
                Console.Write("Test " + Test.Name + ":");
                if(Test.Test())
                {
                    passcount++;
                }
                else
                {
                    failedcount++;
                }
            }
            Console.WriteLine("------------------------------");
            Console.WriteLine("PASSED:" + passcount);
            Console.WriteLine("FAILED:" + failedcount);
            Console.WriteLine("TOTAL:" + testcount);
        }
        private static readonly List<SimpleTest> TestList = new List<SimpleTest>();
        static Tests()
        {
            Type myType = typeof(Tests);
            MemberFilter filter = (MemberInfo m, object? o) =>
            {
                if(m is MethodInfo mi)
                {
                    return mi.ReturnType == typeof(bool) && mi.GetParameters().Length == 0;
                }
                return false;
            };
            MemberInfo[] list = myType.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public, filter, null);
            for(int i = 0; i < list.Length; i++)
            {
                if(list[i] is MethodInfo m)
                {
                    PriorityAttribute? PriorityAttribute = (PriorityAttribute?)Attribute.GetCustomAttribute(m, typeof(PriorityAttribute));
                    int priority = PriorityAttribute?.Priority ?? 0;
                    TestList.Add(new SimpleTest(m.Name, () =>(bool?)m.Invoke(null, null) == true,priority));
                }
            }
        }
        #endregion
        #region Testing Helpers
        public static bool SyncTest(Func<bool> test)
        {
            try
            {
                if (test())
                {
                    Console.WriteLine("PASSED");
                    return true;
                }
                else
                {
                    Console.WriteLine("FAILED");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("EXCEPTION");
                PrintException(e);
            }
            return false;
        }
        public static bool AsyncTest(Func<Task<bool>> test)
        {
            try
            {
                if (Task.Run(async () => await test()).Result)
                {
                    Console.WriteLine("PASSED");
                    return true;
                }
                else
                {
                    Console.WriteLine("FAILED");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION");
                PrintException(e);
            }
            return false;
        }
        public static void PrintException(Exception e, int tabcount = 0)
        {
            if(e == null)
            {
                return;
            }
            Console.WriteLine(new string('\t',tabcount) + e.Message);
            Console.WriteLine(new string('\t', tabcount) + e.StackTrace);
            if (e is AggregateException ae)
            {
                foreach(Exception aesub in ae.InnerExceptions)
                {
                    Console.WriteLine(new string('\t', tabcount) + "InnerException:");
                    PrintException(aesub, tabcount + 1);
                }
                return;
            }
            if (e.InnerException != null)
            {
                Console.WriteLine(new string('\t', tabcount) + "InnerException:");
                PrintException(e.InnerException,tabcount + 1);
            }
        }
        #endregion
        [Priority(int.MaxValue)]
        public static bool TestTest() => SyncTest(()=>true);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PriorityAttribute : Attribute
    {
        public readonly int Priority;
        public PriorityAttribute(int Priority) => this.Priority = Priority;
    }
}
