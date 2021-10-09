using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using CSL.SQL;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Threading;

namespace CSL.Testing
{
    public abstract class Tests
    {
        #region Protected Nested Types
        protected enum TestType { ServerSide, ClientSide, Both};
        protected record TestInstance(string Assembly, string Class, string Name, Func<Task<TestResult?>> Test, int ClassPriority, int Priority, TestType TestType);
        public record TestResult(string Assembly, string Class, string Method, bool? Result, string? Note);
        protected record TestResponse(bool? Result = null, string? Note = null);
        
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        protected class PriorityAttribute : Attribute
        {
            public readonly int Priority;
            public PriorityAttribute(int Priority) => this.Priority = Priority;
        }

        [AttributeUsage(AttributeTargets.Method)]
        protected class TestTypeAttribute : Attribute
        {
            public readonly TestType testType;
            public TestTypeAttribute(TestType testType) => this.testType = testType;
        }
        #endregion
        private static IEnumerable<TestInstance> GetTests()
        {
            Type myType = typeof(Tests);
            foreach (Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(Type Subclass in Assembly.GetTypes().Where((x) => x.IsClass && (x == myType || x.IsSubclassOf(myType))))
                {
                    MemberInfo[] SubclassMemberList = Subclass.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.NonPublic, (MemberInfo m, object? o) =>
                    {
                        if (m is MethodInfo mi)
                        {
                            ParameterInfo[] parameters = mi.GetParameters();
                            
                            return (mi.ReturnType == typeof(Task<TestResponse>) || mi.ReturnType == typeof(TestResponse)) && parameters.Length == 0;
                        }
                        return false;
                    }, null);
                    foreach(MethodInfo mi in SubclassMemberList)
                    {
                        PriorityAttribute? ClassPriorityAttr = (PriorityAttribute?)Attribute.GetCustomAttribute(Subclass, typeof(PriorityAttribute));
                        PriorityAttribute? MethodPriorityAttr = (PriorityAttribute?)Attribute.GetCustomAttribute(mi, typeof(PriorityAttribute));
                        TestTypeAttribute? TestTypeAttr = (TestTypeAttribute?)Attribute.GetCustomAttribute(mi, typeof(TestTypeAttribute));
                        int ClassPriority = ClassPriorityAttr?.Priority ?? 0;
                        int Priority = MethodPriorityAttr?.Priority ?? 0;
                        TestType TestType = TestTypeAttr?.testType ?? TestType.Both;
                        if(mi.ReturnType == typeof(TestResponse))
                        {
                            yield return new TestInstance(Assembly.GetName().Name, Subclass.Name, mi.Name, () =>
                            {
                                try
                                {
                                    object? InvokeResult = mi.Invoke(null, null);
                                    if (InvokeResult is not null and TestResponse response)
                                    {
                                        return Task.FromResult<TestResult?>(new TestResult(Assembly.GetName().Name, Subclass.Name, mi.Name, response.Result, response.Note));
                                    }
                                    return Task.FromResult<TestResult?>(null);
                                }
                                catch(Exception e)
                                {
                                    return Task.FromResult<TestResult?>(new TestResult(Assembly.GetName().Name, Subclass.Name, mi.Name, false, "Exception" + Environment.NewLine + e.ToString()));
                                }
                            }, ClassPriority, Priority, TestType);
                        }
                        if(mi.ReturnType == typeof(Task<TestResponse>))
                        {
                            yield return new TestInstance(Assembly.GetName().Name, Subclass.Name, mi.Name, async () =>
                            {
                                try
                                {
                                    object? InvokeResult = mi.Invoke(null, null);
                                    if (InvokeResult is not null and Task<TestResponse> TaskResponse)
                                    {
                                        TestResponse response = await TaskResponse;
                                        return new TestResult(Assembly.GetName().Name, Subclass.Name, mi.Name, response.Result, response.Note);
                                    }
                                    return null;
                                }
                                catch (Exception e)
                                {
                                    return new TestResult(Assembly.GetName().Name, Subclass.Name, mi.Name, false, "Exception" + Environment.NewLine + e.ToString());
                                }
                            }, ClassPriority, Priority, TestType);
                        }
                    }    
                }
            }
        }
        public async static IAsyncEnumerable<TestResult> RunTests(bool Blazor = false)
        {
            foreach (TestInstance Test in GetTests()
                .Where((x) => Blazor ? x.TestType != TestType.ServerSide : x.TestType != TestType.ClientSide)
                .OrderByDescending((x) => x.ClassPriority).ThenByDescending((x) => x.Priority))
            {
                TestResult? result = await Test.Test();
                if(result != null)
                {
                    yield return result;
                }
            }
            yield break;
        }
        public async static Task<List<TestResult>> RunTests2(bool Blazor = false)
        {
            List<TestResult> toReturn = new List<TestResult>();
            foreach (TestInstance Test in GetTests()
                .Where((x) => Blazor ? x.TestType != TestType.ServerSide : x.TestType != TestType.ClientSide)
                .OrderByDescending((x) => x.ClassPriority).ThenByDescending((x) => x.Priority))
            {
                TestResult? result = await Test.Test();
                if (result != null)
                {
                    toReturn.Add(result);
                }
            }
            return toReturn;
        }
        protected static TestResponse PASS(string? Note = null) => new TestResponse(true, Note);
        protected static TestResponse FAIL(string? Note = null) => new TestResponse(false, Note);
        protected static TestResponse NA(string? Note = null) => new TestResponse(null, Note);
        #region ExampleTests
        [Priority(int.MaxValue)]
        [TestType(TestType.Both)]
        protected static TestResponse SyncTest()
        {
            Thread.Sleep(100);
            return new TestResponse(true);
        }
        [Priority(int.MaxValue)]
        [TestType(TestType.Both)]
        protected static async Task<TestResponse> AsyncTest()
        {
            await Task.Delay(100);
            return new TestResponse(true);
        }
        #endregion
    }
}
