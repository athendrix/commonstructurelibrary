//using System;
//using System.Linq;
//using System.Collections.Generic;
//using System.Text;
//using System.Reflection;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System.Xml;
//using CSL.Helpers;
//using CSL.ClassCreation;
//using System.IO;

//namespace CSL.API
//{
//    public abstract record APIRecord
//    {
//        #region TemplateProcessing
//        public static string CreateRecordFromJSONTemplate(string JSON, string rootName, string Namespace)
//        {
//            CodeGenerator gen = new CodeGenerator();
//            gen.Libraries("CSL.API");
//            gen.BlankLine();
//            gen.Namespace(Namespace);
//            Dictionary<string, Dictionary<string, string?>> RecordInfo = AddRecord(rootName, JObject.Parse(JSON));
//            foreach(KeyValuePair<string, Dictionary<string, string?>> rinfo in RecordInfo)
//            {
//                gen.BeginRecord(rinfo.Key, rinfo.Value.Select(x => new ParameterDefinition(x.Value ?? "string?", x.Key)), " : APIRecord", true);
//            }
//            gen.EndNamespace();
//            return gen.ToString();
//        }
//        public static string CreateRecordFromXMLTemplate(string XML, string rootName, string Namespace)
//        {
//            CodeGenerator gen = new CodeGenerator();
//            gen.Libraries("CSL.API");
//            gen.BlankLine();
//            gen.Namespace(Namespace);
//            Dictionary<string, Dictionary<string, string>> RecordInfo = ScrapeRecordTemplateFromXML(rootName, XML);
//            foreach (KeyValuePair<string, Dictionary<string, string>> rinfo in RecordInfo)
//            {
//                gen.BeginRecord(rinfo.Key, rinfo.Value.Select(x => new ParameterDefinition(x.Value ?? "string?", x.Key)), " : APIRecord", true);
//            }
//            gen.EndNamespace();
//            return gen.ToString();
//        }
//        #region RecordGeneration
//        private record NodeTemplate(string Parent, string Name, bool attribute)
//        {
//            private readonly List<string> Values = new List<string>();
//            private readonly List<NodeTemplate> Children = new List<NodeTemplate>();
//            private readonly List<int> MaxCount = new List<int>();
//            private readonly List<int> MinCount = new List<int>();

//            public void AddChild(NodeTemplate child)
//            {
//                if(child.Parent != Name) { throw new Exception("This child cannot be added to this parent!"); }
//                int ChildIndex = Children.IndexOf(child);
//                if (ChildIndex == -1)
//                {
//                    Children.Add(child);
//                    MaxCount.Add(1);
//                    MinCount.Add(1);
//                }
//                else
//                {
//                    Children[ChildIndex] = MergeChildren(Children[ChildIndex], child);
//                    MaxCount[ChildIndex]++;
//                    MinCount[ChildIndex]++;
//                }
//            }
//            public void AddValue(string value) => Values.Add(value);
//            private static NodeTemplate MergeChildren(List<NodeTemplate> children) => MergeChildren(children.ToArray());
//            private static NodeTemplate MergeChildren(params NodeTemplate[] children)
//            {
//                if (children.Length == 0) { throw new Exception("Length of children cannot be null!"); }
//                string Parent = children[0].Parent;
//                string Name = children[0].Name;
//                bool attribute = children[0].attribute;
//                NodeTemplate toReturn = new NodeTemplate(Parent, Name, attribute);
//                for(int i = 0; i < children.Length; i++)
//                {
//                    if(toReturn != children[i]) { throw new Exception("Cannot merge children of different types"); }
//                    for(int j = 0; j < children[i].Children.Count; j++)
//                    {
//                        int ChildIndex = toReturn.Children.IndexOf(children[i].Children[j]);
//                        if(ChildIndex == -1)
//                        {
//                            toReturn.Children.Add(children[i].Children[j]);
//                            toReturn.MaxCount.Add(children[i].MaxCount[j]);
//                            if (i == 0)
//                            {
//                                toReturn.MinCount.Add(children[i].MinCount[j]);
//                            }
//                            else
//                            {
//                                toReturn.MinCount.Add(0);
//                            }
//                        }
//                        else
//                        {
//                            toReturn.Children[ChildIndex] = MergeChildren(toReturn.Children[ChildIndex], children[i].Children[j]);
//                            toReturn.MaxCount[ChildIndex] = Math.Max(toReturn.MaxCount[ChildIndex], children[i].MaxCount[j]);
//                            toReturn.MinCount[ChildIndex] = Math.Min(toReturn.MinCount[ChildIndex], children[i].MinCount[j]);
//                        }
//                    }
//                    for(int j = 0; j < toReturn.Children.Count; j++)
//                    {
//                        if(!children[i].Children.Contains(toReturn.Children[j]))
//                        {
//                            toReturn.MinCount[j] = 0;
//                        }
//                    }
//                    toReturn.Values.AddRange(children[i].Values);
//                }
//                return toReturn;
//            }

//            public Dictionary<string,Dictionary<string,string>> GetRecordsInfo()
//            {
//                Dictionary<string,Dictionary<string,string>> toReturn = new Dictionary<string,Dictionary<string, string>>();
//                NodeTemplate[] samples = GatherSamples();
//                Dictionary<string, Dictionary<string, string>> Types = new Dictionary<string, Dictionary<string, string>>();
//                int[] NamesCount = new int[samples.Length];
//                for (int i = 0; i < samples.Length; i++)
//                {
//                    NamesCount[i] = samples.Where(x => x.Name == samples[i].Name && x.Children.Any()).Count();
//                    if(!Types.ContainsKey(samples[i].Parent)) { Types.Add(samples[i].Parent, new Dictionary<string, string>()); }
//                    bool forcenullable = samples[i].MinCount.Count == 0 && samples[i].MaxCount.Count == 1;
//                    bool makearray = samples[i].MaxCount.Count > 1;
//                    if (samples[i].Children.Any())
//                    {
//                        string RecName = NamesCount[i] > 1 ? samples[i].Parent + "_" + samples[i].Name : samples[i].Name;
//                        Types[samples[i].Parent].Add(samples[i].Name, RecName + (forcenullable ? "?" : "") + (makearray ? "[]" : ""));
//                    }
//                    else if (samples[i].Values.Count != 0)
//                    {
//                        string TypeString = samples[i].Values.FindTypeString();
//                        if (forcenullable && !TypeString.EndsWith("?")) { TypeString += "?"; }
//                        if (makearray) { TypeString += "[]"; }
//                        if (samples[i].attribute) { TypeString = "[Attribute] " + TypeString; }
//                        Types[samples[i].Parent].Add(samples[i].Name, TypeString );
//                        continue;
//                    }
//                    else
//                    {
//                        Types[samples[i].Parent].Add(samples[i].Name, "<FIXME>");
//                    }
//                }
//                for (int i = 0; i < samples.Length; i++)
//                {
//                    if (samples[i].Children.Any())
//                    {
//                        Dictionary<string, string> toAdd = new Dictionary<string, string>() { };
//                        string RecName = NamesCount[i] > 1 ? samples[i].Parent + "_" + samples[i].Name : samples[i].Name;
//                        for (int j = 0; j < samples[i].Children.Count; j++)
//                        {
//                            toAdd.Add(samples[i].Children[j].Name, Types[samples[i].Name][samples[i].Children[j].Name]);
//                        }
//                        toReturn.Add(RecName, toAdd);
//                    }
//                }
//                return toReturn;
//            }
//            private NodeTemplate[] GatherSamples()
//            {
//                List<NodeTemplate> toReturn = new List<NodeTemplate>();
//                toReturn.Add(this);
//                foreach(NodeTemplate childnode in Children)
//                {
//                    foreach (NodeTemplate child in childnode.GatherSamples())
//                    {
//                        int ChildIndex = toReturn.IndexOf(child);
//                        if (ChildIndex == -1)
//                        {
//                            toReturn.Add(child);
//                        }
//                        else
//                        {
//                            toReturn[ChildIndex] = MergeChildren(toReturn[ChildIndex], child);
//                        }
//                    }
//                }
//                return toReturn.ToArray();
//            }
//            public virtual bool Equals(NodeTemplate? other) => other != null && Parent == other.Parent && Name == other.Name && attribute == other.attribute;
//            public override int GetHashCode() => Parent.GetHashCode() ^ Name.GetHashCode() ^ attribute.GetHashCode();
//        }
//        #endregion
//        #region XMLRecordGeneration
//        private static Dictionary<string, Dictionary<string, string>> ScrapeRecordTemplateFromXML(string name, string XML)
//        {
//            NodeTemplate Root = new NodeTemplate("", name, false);
//            Stack<NodeTemplate> NodeStack = new Stack<NodeTemplate>();
//            NodeStack.Push(Root);
//            using (TextReader tw = new StringReader(XML))
//            using (XmlTextReader reader = new XmlTextReader(tw))
//            {
//                while (reader.Read())
//                {
//                    switch (reader.NodeType)
//                    {
//                        case XmlNodeType.Element:
//                            //Create this node, and add it to its parent.
//                            NodeTemplate Self = new NodeTemplate(NodeStack.Peek().Name, reader.Name, false);
//                            //Process all the Attributes of this Node
//                            for (int i = 0; i < reader.AttributeCount; i++)
//                            {
//                                reader.MoveToAttribute(i);
//                                NodeTemplate Attribute = new NodeTemplate(Self.Name, reader.Name, true);
//                                Attribute.AddValue(reader.Value);
//                                Self.AddChild(Attribute);
//                            }
//                            reader.MoveToElement();
//                            if (!reader.IsEmptyElement)
//                            {
//                                //Make this Node the Parent Node for the next Node
//                                NodeStack.Push(Self);
//                            }
//                            else
//                            {
//                                NodeStack.Peek().AddChild(Self);
//                            }
//                            break;
//                        case XmlNodeType.Text:
//                            NodeStack.Peek().AddValue(reader.Value);
//                            break;
//                        case XmlNodeType.EndElement:
//                            NodeTemplate EndSelf = NodeStack.Pop();
//                            if (reader.Name != EndSelf.Name)
//                            {
//                                Console.WriteLine($"Mismatch! {reader.Name} vs {NodeStack.Peek().Name}");
//                            }
//                            NodeStack.Peek().AddChild(EndSelf);
//                            break;
//                    }
//                }
//            }
//            if(name != NodeStack.Peek().Name)
//            {
//                Console.WriteLine($"Mismatch! {name} vs {NodeStack.Peek().Name}");
//            }
//            return Root.GetRecordsInfo();
//        }
//        #endregion
//        #region JSONRecordGeneration
//        private static Dictionary<string,Dictionary<string,string>> ScrapeRecordTemplateFromJSON(string name, JObject jobj)
//        {
//            return default;
//        }
//        private static Dictionary<string, Dictionary<string, string?>> AddRecord(string name, JObject jobj)
//        {
//            Dictionary<string, Dictionary<string, string?>> toReturn = new Dictionary<string, Dictionary<string, string?>>();
//            AddRecord(name, jobj, toReturn);
//            return toReturn;
//        }
//        private static void AddRecord(string name, JObject jobj, Dictionary<string,Dictionary<string,string?>> ObjectRecord)
//        {
//            if (jobj.Type != JTokenType.Object) { return; }
//            Dictionary<string, string> Items = new Dictionary<string, string>();
//            foreach (KeyValuePair<string, JToken?> item in jobj)
//            {
//                if(item.Value == null) { continue; }
//                string? ValType = ProcessType(item.Key, item.Value, ObjectRecord);
//                if(ValType != null)
//                Items.Add(item.Key, ValType);
//            }
//            if(ObjectRecord.ContainsKey(name))
//            {
//                foreach(KeyValuePair<string, string> item in Items)
//                {
//                    if(ObjectRecord[name].ContainsKey(item.Key))
//                    {
//                        if (ObjectRecord[name][item.Key] != null && item.Value != null && ObjectRecord[name][item.Key] != item.Value)
//                        {
//                            ObjectRecord[name][item.Key] = "string?";
//                        }
//                        if (ObjectRecord[name][item.Key] == null && item.Value != null)
//                        {
//                            ObjectRecord[name][item.Key] = item.Value;
//                        }
//                    }
//                    else
//                    {
//                        ObjectRecord[name][item.Key] = item.Value;
//                    }
//                }
//            }
//            else
//            {
//                ObjectRecord.Add(name, Items);
//            }
//        }
//        private static string? ProcessType(string name, JToken item, Dictionary<string, Dictionary<string, string?>> ObjectRecord)
//        {
//            switch (item.Type)
//            {
//                case JTokenType.None://0
//                    return null;
//                case JTokenType.Object://1
//                    if (item is JObject childobj)
//                    {
//                        AddRecord(name, childobj, ObjectRecord);
//                        return name + "?";
//                    }
//                    break;
//                case JTokenType.Array://2
//                    if (item is JArray arrayobj)
//                    {
//                        JToken? first = arrayobj.First;
//                        if(first != null)
//                        {
//                            string? childPT = ProcessType(name, first, ObjectRecord);
//                            if (childPT != null)
//                            {
//                                return childPT + "[]";
//                            }
//                        }
//                        return null;
//                    }
//                    break;
//                case JTokenType.Constructor://3
//                    return "string?";
//                case JTokenType.Property://4
//                    return "string?";
//                case JTokenType.Comment://5
//                    return null;
//                case JTokenType.Integer://6
//                    return "long?";
//                case JTokenType.Float://7
//                    return "double?";
//                case JTokenType.String://8
//                    return "string?";
//                case JTokenType.Boolean://9
//                    return "bool?";
//                case JTokenType.Null://10
//                    return null;
//                case JTokenType.Undefined://11
//                    return null;
//                case JTokenType.Date://12
//                    return "DateTime?";
//                case JTokenType.Raw://13
//                    return "string?";
//                case JTokenType.Bytes://14
//                    return "byte[]?";
//                case JTokenType.Guid://15
//                    return "Guid?";
//                case JTokenType.Uri://16
//                    return "string?";
//                case JTokenType.TimeSpan://17
//                    return "TimeSpan?";
//            }
//            return null;
//        }
//        #endregion
//        #endregion
//        #region ToJSON
//        private static JToken GetToken(object? obj)
//        {
//            if (obj is APIRecord rec) { return rec.ToJSONObject(); }
//            if (obj is byte[] bytearr) { return new JValue(bytearr.ToStringRT()); }
//            if (obj is Array arr)
//            {
//                JToken[] toReturn = new JToken[arr.Length];
//                for (int i = 0; i < arr.Length; i++)
//                {
//                    toReturn[i] = GetToken(arr.GetValue(i));
//                }
//                return new JArray(toReturn);
//            }
//            if (obj is decimal deci) { return new JValue(deci.ToStringRT()); }
//            return new JValue(obj);
//        }
//        public JObject ToJSONObject()
//        {
//            JObject toReturn = new JObject();
//            Type me = GetType();
//            foreach (ParameterInfo p in GetType().GetConstructors()[0].GetParameters())
//            {
//                PropertyInfo property = me.GetProperty(p.Name);
//                toReturn.Add(p.Name, GetToken(property.GetValue(this)));
//            }
//            return toReturn;
//        }
//        public string ToJSON() => ToJSONObject().ToString();
//        #endregion
//        #region FromJSON
//        private static object? GetValue(JToken token, Type T)
//        {
//            if(token is JValue val)
//            {
//                object? value = val.Value;
//                if(value != null && value.GetType() != T && value.ToStringRT().TryParse(out object? TempVal, T))
//                {
//                    value = TempVal;
//                }
//                return value;
//            }
//            if(token is JArray arr && T.IsArray)
//            {
//                JToken[] toProcess = arr.ToArray();
//                Array valuearr = Array.CreateInstance(T.GetElementType(), toProcess.Length);
//                for (int i = 0; i < toProcess.Length; i++)
//                {
//                    valuearr.SetValue(GetValue(toProcess[i], T.GetElementType()), i);
//                }
//                return valuearr;
//            }
//            if(token is JObject jobj && typeof(APIRecord).IsAssignableFrom(T))
//            {
//                return FromJSONObject(jobj, T);
//            }
//            return null;
//        }
//        public static APIRecord? FromJSONObject(JObject jobj, Type T)
//        {
//            if(!typeof(APIRecord).IsAssignableFrom(T))
//            {
//                throw new Exception("T must be an APIRecord!");
//            }
//            ConstructorInfo ci = T.GetConstructors()[0];//Records always seem to have the default constructor first. (I hope this holds true)
//            ParameterInfo[] TParams = ci.GetParameters();
//            object?[] InvocationParameters = new object[TParams.Length];
//            int HighestParamSet = -1;
//            for (int i = 0; i < TParams.Length; i++)
//            {
//                JToken? token = jobj[TParams[i].Name];
//                if (token == null)
//                {
//                    foreach (JProperty prop in jobj.Properties())
//                    {
//                        if (prop.Name.ToLower() == TParams[i].Name.ToLower())
//                        {
//                            token = prop.Value;
//                            break;
//                        }
//                    }
//                    if (token == null)
//                    {
//                        InvocationParameters[i] = null;
//                        continue;
//                    }
//                }
//                HighestParamSet = i;
//                InvocationParameters[i] = GetValue(token,TParams[i].ParameterType);// switch
//            }
//            return (APIRecord)ci.Invoke(InvocationParameters.AsSpan().Slice(0, HighestParamSet + 1).ToArray());
//        }
//        public static T? FromJSONObject<T>(JObject jobj) where T : APIRecord => (T?)FromJSONObject(jobj, typeof(T));
//        public static T? FromJSON<T>(string JSON) where T : APIRecord => FromJSONObject<T>(JObject.Parse(JSON));
//        #endregion
//    }
//    [AttributeUsage(AttributeTargets.Parameter)]
//    public class AttributeAttribute : Attribute { }
//}
