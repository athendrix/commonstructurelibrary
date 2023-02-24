using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CSL
{
    public abstract record CSLRecord<T> where T : CSLRecord<T>
    {
        public static ParameterInfo[] RecordParameters = typeof(T).GetConstructors()[0].GetParameters();
        public static T GetRecord(object?[] parameters) => (T)typeof(T).GetConstructors()[0].Invoke(parameters);
        public object?[] ToArray()
        {
            if (typeof(T) != GetType()) { throw new Exception("CSLRecord T Type must be the same as the inheriting class!"); }
            object?[] toReturn = new object[RecordParameters.Length];
            for (int i = 0; i < RecordParameters.Length; i++)
            {
                toReturn[i] = typeof(T).GetProperty(RecordParameters[i]?.Name ?? "")?.GetValue(this);
            }
            return toReturn;
        }
    }
}
