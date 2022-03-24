using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CSL.Helpers;

namespace CSL.API
{
    public abstract record APIRecord
    {
        #region ToJSON
        private static JToken GetToken(object? obj)
        {
            if (obj is APIRecord rec) { return rec.ToJSONObject(); }
            if (obj is byte[] bytearr) { return new JValue(bytearr.ToStringRT()); }
            if (obj is Array arr)
            {
                JToken[] toReturn = new JToken[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    toReturn[i] = GetToken(arr.GetValue(i));
                }
                return new JArray(toReturn);
            }
            if (obj is decimal deci) { return new JValue(deci.ToStringRT()); }
            return new JValue(obj);
        }
        public JObject ToJSONObject()
        {
            JObject toReturn = new JObject();
            Type me = GetType();
            foreach (ParameterInfo p in GetType().GetConstructors()[0].GetParameters())
            {
                PropertyInfo property = me.GetProperty(p.Name);
                toReturn.Add(p.Name, GetToken(property.GetValue(this)));
            }
            return toReturn;
        }
        public string ToJSON() => ToJSONObject().ToString();
        #endregion
        #region FromJSON
        private static object? GetValue(JToken token, Type T)
        {
            if(token is JValue val)
            {
                object? value = val.Value;
                if(value != null && value.GetType() != T && value.ToStringRT().TryParse(out object? TempVal, T))
                {
                    value = TempVal;
                }
                return value;
            }
            if(token is JArray arr && T.IsArray)
            {
                JToken[] toProcess = arr.ToArray();
                Array valuearr = Array.CreateInstance(T.GetElementType(), toProcess.Length);
                for (int i = 0; i < toProcess.Length; i++)
                {
                    valuearr.SetValue(GetValue(toProcess[i], T.GetElementType()), i);
                }
                return valuearr;
            }
            if(token is JObject jobj && typeof(APIRecord).IsAssignableFrom(T))
            {
                return FromJSONObject(jobj, T);
            }
            return null;
        }
        public static APIRecord? FromJSONObject(JObject jobj, Type T)
        {
            if(!typeof(APIRecord).IsAssignableFrom(T))
            {
                throw new Exception("T must be an APIRecord!");
            }
            ConstructorInfo ci = T.GetConstructors()[0];//Records always seem to have the default constructor first. (I hope this holds true)
            ParameterInfo[] TParams = ci.GetParameters();
            object?[] InvocationParameters = new object[TParams.Length];
            int HighestParamSet = -1;
            for (int i = 0; i < TParams.Length; i++)
            {
                JToken? token = jobj[TParams[i].Name];
                if (token == null)
                {
                    foreach (JProperty prop in jobj.Properties())
                    {
                        if (prop.Name.ToLower() == TParams[i].Name.ToLower())
                        {
                            token = prop.Value;
                            break;
                        }
                    }
                    if (token == null)
                    {
                        InvocationParameters[i] = null;
                        continue;
                    }
                }
                HighestParamSet = i;
                InvocationParameters[i] = GetValue(token,TParams[i].ParameterType);// switch
            }
            return (APIRecord)ci.Invoke(InvocationParameters.AsSpan().Slice(0, HighestParamSet + 1).ToArray());
        }
        public static T? FromJSONObject<T>(JObject jobj) where T : APIRecord => (T?)FromJSONObject(jobj, typeof(T));
        public static T? FromJSON<T>(string JSON) where T : APIRecord => FromJSONObject<T>(JObject.Parse(JSON));
        #endregion
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class AttributeAttribute : Attribute { }
}
