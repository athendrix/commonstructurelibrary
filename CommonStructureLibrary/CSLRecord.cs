using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CSL
{
    public abstract record CSLRecord<T> where T : CSLRecord<T>
    {
        private static bool IsValidType(Type t) =>
            t == typeof(bool) || t == typeof(bool?) ||
            t == typeof(sbyte) || t == typeof(sbyte?) ||
            t == typeof(byte) || t == typeof(byte?) ||
            t == typeof(char) || t == typeof(char?) ||
            t == typeof(short) || t == typeof(short?) ||
            t == typeof(ushort) || t == typeof(ushort?) ||
            t == typeof(int) || t == typeof(int?) ||
            t == typeof(uint) || t == typeof(uint?) ||
            t == typeof(long) || t == typeof(long?) ||
            t == typeof(ulong) || t == typeof(ulong?) ||
            t == typeof(float) || t == typeof(float?) ||
            t == typeof(double) || t == typeof(double?) ||
            t == typeof(decimal) || t == typeof(decimal?) ||
            t == typeof(Guid) || t == typeof(Guid?) ||
            t == typeof(DateTime) || t == typeof(DateTime?) ||
            t == typeof(string) ||
            t == typeof(byte[]) ||
            t.IsEnum || Nullable.GetUnderlyingType(t).IsEnum;
        static CSLRecord()
        {
            foreach(ParameterInfo pi in RecordParameters)
            {
                if(!IsValidType(pi.ParameterType))
                {
                    throw new Exception("CSLRecords only support basic data types at the moment. This limitation allows for extended capabilities.");
                }
            }
        }
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
