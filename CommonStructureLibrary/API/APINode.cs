//using System;
//using System.Collections.Generic;
//using System.Text;
//using Newtonsoft.Json;
//using System.IO;
//using System.Xml;
//using CSL.Helpers;

//namespace CSL.API
//{
//    /// <summary>
//    /// Designed to be an intersection of APIRecords, JSON, and XML, allowing conversion between the 3 different forms.
//    /// It supports 8 datatypes:
//    /// String
//    /// LongInteger which also can convert to other integer based numeric types
//    /// Double which can also convert to other IEEE floating point types
//    /// Object which can contain any of the supported types
//    /// Array of any of the supported types
//    /// Boolean
//    /// DateTime
//    /// ByteArray which is handled as a special case of Webfriendly Base64 (replacing the + with - and the / with _ and trimming the = off the end.
//    /// 
//    /// There are some issues with recognising certain data types when starting from JSON or XML data, as these do not have the mechanisms to fully define all available datatypes.
//    /// (XML cannot define datatypes at all, but can define something as an "attribute", JSON cannot define ByteArrays, and neither can define nullability of a value) 
//    /// Despite this, the conversion mechanism should still work.
//    /// For example, If you have a ByteArray Base64 value in XML, and convert directly to JSON,
//    /// there's no way to know it's a Base64 value, so it will pass it as a string, which is still valid.
//    /// 
//    /// Because of this though, we recommend creating records first, marking XML attributes with the "Attribute" Attribute, and converting everything to and from records.
//    /// If you are starting with JSON or XML, basic records can be created from example data, with missing information able to be filled in later.
//    /// </summary>
//    /// <param name="name"></param>
//    /// <param name="value"></param>
//    /// <param name="NodeType"></param>
//    /// <param name="children"></param>
//    /// <param name="Nullable"></param>
//    /// <param name="Attribute"></param>
//    public record APINode(string name, string? value, APINodeType NodeType, APINode[] children, bool Nullable, bool Attribute)
//    {
//        public static APINode FromJSON(string JSON)
//        {
//            Stack<string> ObjectNames = new Stack<string>();
//            Stack<List<APINode>> NodeStack = new Stack<List<APINode>>();
//            string LastName = "ROOT";
//            ObjectNames.Push(LastName);
//            NodeStack.Push(new List<APINode>());
//            using(TextReader tw = new StringReader(JSON))
//            using (JsonTextReader reader = new JsonTextReader(tw))
//            {
//                while(reader.Read())
//                {
//                    switch(reader.TokenType)
//                    {
//                        case JsonToken.None://0
//                            break;
//                        case JsonToken.StartObject://1
//                        case JsonToken.StartArray://2
//                            ObjectNames.Push(LastName);
//                            NodeStack.Push(new List<APINode>());
//                            break;
//                        case JsonToken.StartConstructor://3
//                            throw new Exception("Example JSON must not have constructors!");
//                        case JsonToken.PropertyName://4
//                            LastName = reader.Value?.ToString() ?? "";
//                            break;
//                        case JsonToken.Comment://5
//                            break;
//                        case JsonToken.Raw://6
//                            NodeStack.Peek().Add(FromJSON(reader.Value?.ToString() ?? ""));
//                            break;
//                        case JsonToken.Integer://7
//                            NodeStack.Peek().Add(new APINode(LastName, reader.Value?.ToString(), APINodeType.LongInteger, new APINode[0], true, false));
//                            break;
//                        case JsonToken.Float://8
//                            NodeStack.Peek().Add(new APINode(LastName, reader.Value?.ToString(), APINodeType.Double, new APINode[0], true, false));
//                            break;
//                        case JsonToken.String://9
//                            NodeStack.Peek().Add(new APINode(LastName, reader.Value?.ToString(), APINodeType.String, new APINode[0], true, false));
//                            break;
//                        case JsonToken.Boolean://10
//                            NodeStack.Peek().Add(new APINode(LastName, reader.Value?.ToString(), APINodeType.Boolean, new APINode[0], true, false));
//                            break;
//                        case JsonToken.Null://11
//                        case JsonToken.Undefined://12
//                            NodeStack.Peek().Add(new APINode(LastName, reader.Value?.ToString(), APINodeType.Unknown, new APINode[0], true, false));
//                            break;
//                        case JsonToken.EndObject://13
//                            {
//                                APINode[] children = NodeStack.Pop().ToArray();
//                                string ObjectName = ObjectNames.Pop();
//                                NodeStack.Peek().Add(new APINode(ObjectName, null, APINodeType.Object, children, true, false));
//                            }
//                            break;
//                        case JsonToken.EndArray://14
//                            {
//                                APINode[] children = NodeStack.Pop().ToArray();
//                                string ObjectName = ObjectNames.Pop();
//                                NodeStack.Peek().Add(new APINode(ObjectName, null, APINodeType.Array, children, true, false));
//                            }
//                            break;
//                        case JsonToken.EndConstructor://15
//                            throw new Exception("Example JSON must not have constructors!");
//                        case JsonToken.Date://16
//                            NodeStack.Peek().Add(new APINode(LastName, reader.Value?.ToString(), APINodeType.DateTime, new APINode[0], true, false));
//                            break;
//                    }
//                }
//            }
//            List<APINode> RootList = NodeStack.Pop();
//            if(RootList.Count == 0) {return new APINode("ROOT", null, APINodeType.Object, new APINode[0], true, false);}
//            APINode toReturn = new APINode("ROOT", null, APINodeType.Object, RootList.ToArray(), true, false);
//            // Probably a bug
//            // while (toReturn.children.Length == 1) { toReturn = toReturn.children[0]; }
//            return toReturn;
//        }
//        private static APINodeType ParseTypeFromString(string data)
//        {
//            if (Generics.TryParse(data, out long? testint) && testint != null) { return APINodeType.LongInteger; }
//            if (Generics.TryParse(data, out double? testdouble) && testdouble != null) { return APINodeType.Double; }
//            if (Generics.TryParse(data, out bool? testbool) && testbool != null) { return APINodeType.Boolean; }
//            if (Generics.TryParse(data, out DateTime? testDT) && testDT != null) { return APINodeType.DateTime; }
//            return APINodeType.String;
//        }
//        public static APINode FromXML(string XML)
//        {
//            Stack<string> ObjectNames = new Stack<string>();
//            Stack<List<APINode>> NodeStack = new Stack<List<APINode>>();
//            string LastName = "ROOT";
//            ObjectNames.Push(LastName);
//            NodeStack.Push(new List<APINode>());
//            using (TextReader tw = new StringReader(XML))
//            using (XmlTextReader reader = new XmlTextReader(tw))
//            {
//                while (reader.Read())
//                {
//                    //Console.WriteLine(reader.NodeType.ToString() + "|" + reader.Name + "|" + reader.Value + "|" + reader.ValueType);
//                    switch (reader.NodeType)
//                    {
//                        case XmlNodeType.Element:
//                            LastName = String.IsNullOrWhiteSpace(reader.Name) ? LastName : reader.Name;
//                            ObjectNames.Push(LastName);
//                            NodeStack.Push(new List<APINode>());
//                            for(int i = 0; i < reader.AttributeCount; i++)
//                            {
//                                reader.MoveToAttribute(i);
//                                //Console.WriteLine(reader.NodeType.ToString() + "|" + reader.Name + "|" + reader.Value + "|" + reader.ValueType);
//                                NodeStack.Peek().Add(new APINode(reader.Name, reader.Value, ParseTypeFromString(reader.Value), new APINode[0], true, true));
//                            }
//                        break;
//                        case XmlNodeType.Text:
//                            LastName = String.IsNullOrWhiteSpace(reader.Name) ? LastName : reader.Name;
//                            NodeStack.Peek().Add(new APINode(LastName, reader.Value, ParseTypeFromString(reader.Value), new APINode[0], true, false));
//                            break;
//                        case XmlNodeType.EndElement:
//                            string name = ObjectNames.Pop();
//                            List<APINode> nodes = NodeStack.Pop();
//                            if(nodes.Count == 0)
//                            {
//                                continue;
//                            }
//                            if(nodes.Count == 1 && nodes[0].Attribute == false)
//                            {
//                                NodeStack.Peek().Add(nodes[0]);
//                            }
//                            else
//                            {
//                                NodeStack.Peek().Add(new APINode(name, null, APINodeType.Object, nodes.ToArray(), true, false));
//                            }
//                        break;
//                    }
//                }
//            }
//            List<APINode> RootList = NodeStack.Pop();
//            if (RootList.Count == 0) { return new APINode("ROOT", null, APINodeType.Object, new APINode[0], true, false); }
//            APINode toReturn = new APINode("ROOT", null, APINodeType.Object, RootList.ToArray(), true, false);
//            while (toReturn.children.Length == 1) { toReturn = toReturn.children[0]; }
//            return toReturn;
//        }
//    }
//    public enum APINodeType
//    {
//        String,
//        LongInteger,
//        Double,
//        Object,
//        Array,
//        Boolean,
//        DateTime,
//        ByteArray,
//        Unknown
//    }
//}
