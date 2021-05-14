using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSL.SQL.ClassCreator
{
    public class Column
    {
        public static readonly Dictionary<string, ColumnType> TypeMap = new Dictionary<string, ColumnType>()
        {
            {"BOOL",ColumnType.Boolean},
            {"BOOLEAN",ColumnType.Boolean},

            {"BYTE", ColumnType.Byte},

            {"CHAR",ColumnType.Char},

            {"SHORT", ColumnType.Short},
            {"INT16", ColumnType.Short},
            {"USHORT", ColumnType.Integer},
            {"UINT16", ColumnType.Integer},

            {"INT",ColumnType.Integer},
            {"INTEGER",ColumnType.Integer},
            {"INT32",ColumnType.Integer},
            {"UINT",ColumnType.Long},
            {"UINT32",ColumnType.Long},

            {"LONG",ColumnType.Long},
            {"INT64",ColumnType.Long},
            {"ULONG", ColumnType.UnsignedLong},
            {"UINT64", ColumnType.UnsignedLong},

            {"FLOAT", ColumnType.Float},
            {"DOUBLE", ColumnType.Double},
            {"DECIMAL", ColumnType.Decimal},
            {"NUMERIC", ColumnType.Decimal},

            {"CHAR[]",ColumnType.String},
            {"STRING",ColumnType.String},
            {"VARCHAR",ColumnType.String},
            {"TEXT",ColumnType.String},

            {"BYTE[]",ColumnType.ByteArray},
            {"BYTEA",ColumnType.ByteArray},

            {"GUID",ColumnType.Guid},
            {"UUID",ColumnType.Guid},

            {"DATETIME",ColumnType.DateTime},
            {"DT",ColumnType.DateTime},
            {"TIMESTAMP",ColumnType.DateTime},

            {"ENUM", ColumnType.Enum},

            {"STRUCT", ColumnType.Struct},

            {"CLASS", ColumnType.Class },
        };
        public static Regex LengthFinder = new Regex(@"\((\d+)\)", RegexOptions.Compiled);
        public readonly string ColumnName;
        public readonly ColumnType type;
        public readonly bool nullable;
        public readonly int length;

        public string CSharpTypeName
        {
            get
            {
                switch(type)
                {
                    case ColumnType.Boolean: return "bool" + (nullable ? "?" : "");
                    case ColumnType.Byte: return "byte" + (nullable ? "?" : "");
                    case ColumnType.Char: return "char" + (nullable ? "?" : "");
                    case ColumnType.Short: return "short" + (nullable ? "?" : "");
                    case ColumnType.Integer: return "int" + (nullable ? "?" : "");
                    case ColumnType.Long: return "long" + (nullable ? "?" : "");
                    case ColumnType.UnsignedLong: return "ulong" + (nullable ? "?" : "");
                    case ColumnType.Float: return "float" + (nullable ? "?" : "");
                    case ColumnType.Double: return "double" + (nullable ? "?" : "");
                    case ColumnType.Decimal: return "decimal" + (nullable ? "?" : "");
                    case ColumnType.String: return "string";
                    case ColumnType.ByteArray: return "byte[]";
                    case ColumnType.Guid: return "Guid" + (nullable ? "?" : "");
                    case ColumnType.DateTime: return "DateTime" + (nullable ? "?" : "");
                    case ColumnType.Enum: return ColumnName + (nullable ? "?" : "");
                    case ColumnType.Struct: return ColumnName + (nullable ? "?" : "");
                    case ColumnType.Class: return ColumnName;
                    default: return "<FIXME>";
                }
            }
        }
        public string CSharpPrivateTypeName
        {
            get
            {
                switch (type)
                {
                    case ColumnType.Byte: return "byte[]";
                    case ColumnType.UnsignedLong: return "long" + (nullable ? "?" : "");
                    case ColumnType.Enum: return "long" + (nullable ? "?" : "");
                    case ColumnType.Struct: return "byte[]";
                    case ColumnType.Class: return "byte[]";
                    default: return CSharpTypeName;
                }
            }
        }
        #region SpecialConversions
        public string CSharpConvertPrivatePrepend
        {
            get
            {
                switch (type)
                {
                    case ColumnType.Byte: return "new byte[] {";
                    case ColumnType.UnsignedLong: return "(long" + (nullable ? "?" : "") + ")";
                    case ColumnType.Enum: return "(long" + (nullable ? "?" : "") + ")";
                    //ColumnType.Struct : return "MemoryMarshal.AsBytes(new " + CSharpTypeName.TrimEnd('?') + "[]{";
                    default: return "";
                }
            }
        }
        public string CSharpConvertPrivateAppend
        {
            get
            {
                switch (type)
                {
                    case ColumnType.Byte: return "}";
                    //case ColumnType.UnsignedLong : return (nullable?".Value":"") + ".ToLong()";
                    //case ColumnType.Enum : return ")" + (nullable?".Value":"") + ".ToLong()";
                    case ColumnType.Struct: return (nullable ? ".Value" : "") + ".ToByteArray()";
                    case ColumnType.Class: return ".ToByteArray()";
                    default: return "";
                }
            }
        }
        public string CSharpConvertPublicPrepend
        {
            get
            {
                switch (type)
                {
                    case ColumnType.Enum: return "(" + CSharpTypeName + ")";
                    case ColumnType.UnsignedLong: return "(ulong" + (nullable ? "?" : "") + ")";
                    //case ColumnType.Struct : return "MemoryMarshal.AsRef<" + CSharpTypeName.TrimEnd('?') + ">(";
                    case ColumnType.Class: return "new " + CSharpTypeName + "(";
                    default: return "";
                }
            }
        }
        public string CSharpConvertPublicAppend
        {
            get
            {
                switch (type)
                {
                    case ColumnType.Byte: return "[0]";
                    //case ColumnType.UnsignedLong : return (nullable?"?":"") + ".ToUlong()";
                    //case ColumnType.Enum : return (nullable?"?":"") + ".ToUlong())";
                    case ColumnType.Struct: return ".ToStruct<" + CSharpTypeName.TrimEnd('?') + ">()";
                    case ColumnType.Class: return ")";
                    default: return "";
                }
            }
        }
        #endregion
        public string SQLTypeName
        {
            get
            {
                switch (type)
                {
                    case ColumnType.Boolean: return "BOOLEAN" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Byte: return "BYTEA" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Char: return "CHAR(1)" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Short: return "SMALLINT" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Integer: return "INTEGER" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Long: return "BIGINT" + (nullable ? "" : " NOT NULL");
                    case ColumnType.UnsignedLong: return "BIGINT" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Float: return "FLOAT4" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Double: return "FLOAT8" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Decimal: return "NUMERIC" + (nullable ? "" : " NOT NULL");
                    case ColumnType.String: return (length >= 0 ? "VARCHAR(" + length.ToString() + ")" : "TEXT") + (nullable ? "" : " NOT NULL");
                    case ColumnType.ByteArray: return "BYTEA" + (nullable ? "" : " NOT NULL");//TODO: Add Length
                    case ColumnType.Guid: return "UUID" + (nullable ? "" : " NOT NULL");
                    case ColumnType.DateTime: return "TIMESTAMP" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Enum: return "BIGINT" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Struct: return "BYTEA" + (nullable ? "" : " NOT NULL");
                    case ColumnType.Class: return "BYTEA" + (nullable ? "" : " NOT NULL");
                    default: return "<FIXME>";
                }
            }
        }

        public Column(string Name, string StringType)
        {
            ColumnName = Common.NameParser(Name).Replace(" ", "");
            nullable = false;
            length = -1;
            StringType = StringType.ToUpper().Replace(" ", "").Trim();
            if (StringType.Contains('?'))
            {
                nullable = true;
                StringType = StringType.Replace("?", "");
            }
            Match m;
            if ((m = LengthFinder.Match(StringType)).Success)
            {
                string StrMaxLength = null;
                foreach(Group g in m.Groups)
                {
                    StrMaxLength = g.Value;
                }
                StringType = LengthFinder.Replace(StringType, "");
                if (StrMaxLength != null)
                {
                    length = int.Parse(StrMaxLength);
                }
            }
            if (TypeMap.ContainsKey(StringType))
            {
                type = TypeMap[StringType];
            }
            else
            {
                type = ColumnType.Unknown;
            }
        }
    }
    public enum ColumnType
    {
        Boolean,               //BOOLEAN
        Byte,                  //BYTEA(1)
        Char,                  //CHAR(1) //Not a varchar, because there's only 1
        Short,                 //SMALLINT
        Integer,               //INTEGER
        Long,                  //BIGINT
        UnsignedLong,          //BIGINT with C# conversion
        Float,                 //FLOAT4
        Double,                //FLOAT8
        Decimal,               //NUMERIC

        String,                //VARCHAR with limit or TEXT without
        ByteArray,             //BYTEA
        Guid,                  //UUID
        DateTime,              //TIMESTAMP
        Enum, //Unsigned Long for maximum values. //BIGINT with C# conversion
        //A User defined datatype that won't be portable across archetectures.
        //x86 vs ARM
        //Dependant on the ability of Memory Marshal to do the conversion.
        Struct,                //BYTEA
        Class,
        Unknown
    }
}
