using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using CSL.Helpers;
using CSL.ClassCreation;

namespace CSL.API
{
    public abstract record APIRecord
    {
        #region TemplateProcessing
        public static string CreateRecordFromJSONTemplate(string JSON, string rootName, string Namespace)
        {
            CodeGenerator gen = new CodeGenerator();
            gen.Libraries("CSL.API");
            gen.BlankLine();
            gen.Namespace(Namespace);
            Dictionary<string, Dictionary<string, string?>> RecordInfo = AddRecord(rootName, JObject.Parse(JSON));
            foreach(KeyValuePair<string, Dictionary<string, string?>> rinfo in RecordInfo)
            {
                gen.BeginRecord(rinfo.Key, rinfo.Value.Select(x => new ParameterDefinition(x.Value ?? "string?", x.Key)), " : APIRecord", true);
            }
            gen.EndNamespace();
            return gen.ToString();
        }
        public static string CreateRecordFromXMLTemplate(string XML, string rootName, string Namespace)
        {
#warning Unfinished
            CodeGenerator gen = new CodeGenerator();
            gen.Libraries("CSL.API");
            gen.BlankLine();
            gen.Namespace(Namespace);
            Dictionary<string, Dictionary<string, string?>> RecordInfo = AddRecord(rootName, JObject.Parse(JSON));
            foreach (KeyValuePair<string, Dictionary<string, string?>> rinfo in RecordInfo)
            {
                gen.BeginRecord(rinfo.Key, rinfo.Value.Select(x => new ParameterDefinition(x.Value ?? "string?", x.Key)), " : APIRecord", true);
            }
            gen.EndNamespace();
            return gen.ToString();
        }
        #region JSONRecordGeneration
        private static Dictionary<string, Dictionary<string, string?>> AddRecord(string name, JObject jobj)
        {
            Dictionary<string, Dictionary<string, string?>> toReturn = new Dictionary<string, Dictionary<string, string?>>();
            AddRecord(name, jobj, toReturn);
            return toReturn;
        }
        private static void AddRecord(string name, JObject jobj, Dictionary<string,Dictionary<string,string?>> ObjectRecord)
        {
            if (jobj.Type != JTokenType.Object) { return; }
            Dictionary<string, string?> Items = new Dictionary<string, string?>();
            foreach (KeyValuePair<string, JToken?> item in jobj)
            {
                if(item.Value == null) { continue; }
                Items.Add(item.Key, ProcessType(item.Key, item.Value, ObjectRecord));
            }
            if(ObjectRecord.ContainsKey(name))
            {
                foreach(KeyValuePair<string, string?> item in Items)
                {
                    if(ObjectRecord[name].ContainsKey(item.Key))
                    {
                        if (ObjectRecord[name][item.Key] != null && item.Value != null && ObjectRecord[name][item.Key] != item.Value)
                        {
                            ObjectRecord[name][item.Key] = "string?";
                        }
                        if (ObjectRecord[name][item.Key] == null && item.Value != null)
                        {
                            ObjectRecord[name][item.Key] = item.Value;
                        }
                    }
                    else
                    {
                        ObjectRecord[name][item.Key] = item.Value;
                    }
                }
            }
            else
            {
                ObjectRecord.Add(name, Items);
            }
            
        }
        private static string? ProcessType(string name, JToken item, Dictionary<string, Dictionary<string, string?>> ObjectRecord)
        {
            switch (item.Type)
            {
                case JTokenType.None://0
                    return null;
                case JTokenType.Object://1
                    if (item is JObject childobj)
                    {
                        AddRecord(name, childobj, ObjectRecord);
                        return name + "?";
                    }
                    break;
                case JTokenType.Array://2
                    if (item is JArray arrayobj)
                    {
                        JToken? first = arrayobj.First;
                        if(first != null)
                        {
                            string? childPT = ProcessType(name, first, ObjectRecord);
                            if (childPT != null)
                            {
                                return childPT + "[]";
                            }
                        }
                        return null;
                    }
                    break;
                case JTokenType.Constructor://3
                    return "string?";
                case JTokenType.Property://4
                    return "string?";
                case JTokenType.Comment://5
                    return null;
                case JTokenType.Integer://6
                    return "long?";
                case JTokenType.Float://7
                    return "double?";
                case JTokenType.String://8
                    return "string?";
                case JTokenType.Boolean://9
                    return "bool?";
                case JTokenType.Null://10
                    return null;
                case JTokenType.Undefined://11
                    return null;
                case JTokenType.Date://12
                    return "DateTime?";
                case JTokenType.Raw://13
                    return "string?";
                case JTokenType.Bytes://14
                    return "byte[]?";
                case JTokenType.Guid://15
                    return "Guid?";
                case JTokenType.Uri://16
                    return "string?";
                case JTokenType.TimeSpan://17
                    return "TimeSpan?";
            }
            return null;
        }
        #endregion
        #endregion
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
