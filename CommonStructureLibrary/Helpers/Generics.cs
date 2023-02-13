using System;
using System.Collections.Generic;
using System.Reflection;

namespace CSL.Helpers
{
    public static class Generics
    {
        #region TryParse
        /// <summary>
        /// Tries to parse the string s as the given TType.
        /// If successful, will return true, otherwise it will return false.
        /// The goal is that for any value converted to a string using the ToStringRT function, this will return the original value.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="result">The resulting object.</param>
        /// <param name="TType">The type to parse into.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool TryParse(this string? s, out object? result, Type TType)
        {
            bool toReturn;
            //Type TType = typeof(T?);
            #region Special Cases
            if(TType == typeof(void))
            {
                throw new ArgumentException("Cannot parse a string to void!");
            }
            if (TType == typeof(string) || TType == typeof(object))
            {
                result = s;
                return true;
            }
            if (s is "" && TType == typeof(byte[]))
            {
                result = new byte[0];
                return true;
            }
            if (s is "" && (TType == typeof(Guid) || TType == typeof(Guid?)))
            {
                result = Guid.Empty;
                return true;
            }
            if (s is "" or null)
            {
                if (TType.IsValueType)
                {
                    result = Activator.CreateInstance(TType);
                }
                else
                {
                    result = null;
                }
                return result == null;
            }
            #endregion
            #region Signed Integers
            if (TType == typeof(sbyte) || TType == typeof(sbyte?))
            {
                toReturn = sbyte.TryParse(s, out sbyte Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(short) || TType == typeof(short?))
            {
                toReturn = short.TryParse(s, out short Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(int) || TType == typeof(int?))
            {
                toReturn = int.TryParse(s, out int Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(long) || TType == typeof(long?))
            {
                toReturn = long.TryParse(s, out long Tresult);
                result = Tresult;
                return toReturn;
            }
            #endregion
            #region Unsigned Integers
            if (TType == typeof(byte) || TType == typeof(byte?))
            {
                toReturn = byte.TryParse(s, out byte Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(ushort) || TType == typeof(ushort?))
            {
                toReturn = ushort.TryParse(s, out ushort Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(uint) || TType == typeof(uint?))
            {
                toReturn = uint.TryParse(s, out uint Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(ulong) || TType == typeof(ulong?))
            {
                toReturn = ulong.TryParse(s, out ulong Tresult);
                result = Tresult;
                return toReturn;
            }
            #endregion
            #region Floating Point and Decimal
            if (TType == typeof(float) || TType == typeof(float?))
            {
                toReturn = float.TryParse(s, out float Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(double) || TType == typeof(double?))
            {
                toReturn = double.TryParse(s, out double Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(decimal) || TType == typeof(decimal?))
            {
                toReturn = decimal.TryParse(s, out decimal Tresult);
                result = Tresult;
                return toReturn;
            }
            #endregion
            #region Other
            if (TType == typeof(bool) || TType == typeof(bool?))
            {
                string uppers = s.ToUpper();
                if (uppers is "0" or "F" or "N" or "NO")
                {
                    result = false;
                    return true;
                }
                if (uppers is "1" or "T" or "Y" or "YES")
                {
                    result = true;
                    return true;
                }
                toReturn = bool.TryParse(s, out bool Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(Guid) || TType == typeof(Guid?))
            {
                toReturn = Guid.TryParse(s, out Guid Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(DateTime) || TType == typeof(DateTime?))
            {
                toReturn = DateTime.TryParse(s, out DateTime Tresult);
                Tresult = Tresult.ToUniversalTime();
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(char) || TType == typeof(char?))
            {
                toReturn = char.TryParse(s, out char Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType == typeof(TimeSpan) || TType == typeof(TimeSpan?))
            {
                toReturn = TimeSpan.TryParse(s, out TimeSpan Tresult);
                result = Tresult;
                return toReturn;
            }
            if (TType.IsEnum)
            {
                try
                {
                    toReturn = true;
                    object? Tresult;
                    try
                    {
                        Tresult = Enum.Parse(TType, s, true);
                    }
                    catch (Exception)
                    {
                        toReturn = false;
                        Tresult = Activator.CreateInstance(TType);
                    }
                    result = Tresult;
                    return toReturn;
                }
                catch (Exception)
                {
                    result = Activator.CreateInstance(TType);
                    return false;
                }
            }
            if (TType == typeof(byte[]))
            {
                try
                {
                    int len = s.Length;
                    //Convert the different kinds of Base64 to the common type (using + and / and padding with = if necessary)
                    string temps = s.Replace('-', '+').Replace('_', '/').PadRight(len + ((4 - (len % 4)) % 4), '=');
                    result = Convert.FromBase64String(temps);
                    return true;
                }
                catch (Exception)
                {
                    result = null;
                    return false;
                }
            }
            #endregion
            #region Serialization
            try
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject(s,TType);
                return true;
            }
            catch { }
            #endregion
            if (TType.IsValueType)
            {
                result = Activator.CreateInstance(TType);
            }
            else
            {
                result = null;
            }
            return false;
        }
        /// <summary>
        /// A typesafe version of the TryParse function.
        /// Tries to parse the string s as the given Type T.
        /// If successful, will return true, otherwise it will return false.
        /// The goal is that for any value converted to a string using the ToStringRT function, this will return the original value.
        /// </summary>
        /// <typeparam name="T">The type to parse into.</typeparam>
        /// <param name="s">The string to parse.</param>
        /// <param name="result">The resulting T.</param>
        /// <returns></returns>
        public static bool TryParse<T>(this string? s, out T? result)
        {
            bool toReturn = TryParse(s, out object? objresult, typeof(T?));
            result = (T?)objresult;
            return toReturn;
        }
        #endregion
        #region ToStringRT
        /// <summary>
        /// Mostly the same as the built-in ToString function.
        /// But for certain values, it will provide a better roundtripable default.
        /// The goal is that for any value converted to a string using this ToString function, you should be able to get the original value back with the TryParse of the same type.
        /// </summary>
        /// <param name="input">The value to convert into a string.</param>
        /// <returns>The string version of the value.</returns>
        public static string? ToString(object? input)
        {
            if (input == null)
            {
                return null;
            }
            Type TType = input.GetType();
            if (TType == typeof(string))
            {
                return (string)input;
            }
            #region Signed Integers
            if (TType == typeof(sbyte) || TType == typeof(sbyte?))
            {
                return ((sbyte)input).ToString();
            }
            if (TType == typeof(short) || TType == typeof(short?))
            {
                return ((short)input).ToString();
            }
            if (TType == typeof(int) || TType == typeof(int?))
            {
                return ((int)input).ToString();
            }
            if (TType == typeof(long) || TType == typeof(long?))
            {
                return ((long)input).ToString();
            }
            #endregion
            #region Unsigned Integers
            if (TType == typeof(byte) || TType == typeof(byte?))
            {
                return ((byte)input).ToString();
            }
            if (TType == typeof(ushort) || TType == typeof(ushort?))
            {
                return ((ushort)input).ToString();
            }
            if (TType == typeof(uint) || TType == typeof(uint?))
            {
                return ((uint)input).ToString();
            }
            if (TType == typeof(ulong) || TType == typeof(ulong?))
            {
                return ((ulong)input).ToString();
            }
            #endregion
            #region Floating Point and Decimal
            if (TType == typeof(float) || TType == typeof(float?))
            {
                return ((float)input).ToString("R");
            }
            if (TType == typeof(double) || TType == typeof(double?))
            {
                return ((double)input).ToString("R");
            }
            if (TType == typeof(decimal) || TType == typeof(decimal?))
            {
                return ((decimal)input).ToString();
            }
            #endregion
            #region Other
            if (TType == typeof(bool) || TType == typeof(bool?))
            {
                return ((bool)input).ToString();
            }
            if (TType == typeof(Guid) || TType == typeof(Guid?))
            {
                return ((Guid)input).ToString();
            }
            if (TType == typeof(DateTime) || TType == typeof(DateTime?))
            {
                return ((DateTime)input).ToUniversalTime().ToString("O");
            }
            if (TType == typeof(char) || TType == typeof(char?))
            {
                return ((char)input).ToString();
            }
            if (TType == typeof(TimeSpan) || TType == typeof(TimeSpan?))
            {
                return ((TimeSpan)input).ToString("G");
            }
            if (TType.IsEnum)
            {
                return Enum.GetName(TType, input);
            }
            if (TType == typeof(byte[]))
            {
                return ((byte[])input).EncodeToWebBase64();
            }
            #endregion
            #region Serialization
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(input);
            }
            catch { }
            #endregion
            return input.ToString();
        }
        /// <summary>
        /// Mostly the same as the built-in ToString function.
        /// But for certain values, it will provide a better roundtripable default.
        /// The goal is that for any value converted to a string using this ToStringRT function, you should be able to get the original value back with the TryParse of the same type.
        /// </summary>
        /// <param name="input">The value to convert into a string.</param>
        /// <returns>The string version of the value.</returns>
        public static string? ToStringRT(this object? input) => ToString(input);
        #endregion
        #region BasicType
        public static Type FindType(this IEnumerable<string?> strings) => FindBasicType(strings, out bool nullable).FromBasicType(nullable);
        public static string FindTypeString(this IEnumerable<string?> strings) => FindBasicType(strings, out bool nullable).ToString(nullable);
        public static BasicType FindBasicType(this IEnumerable<string?> strings, out bool nullable)
        {

            bool[] validTypes = new bool[(int)BasicType.String];
            bool[] nullvalues = new bool[(int)BasicType.String];
            bool[] nonnullvalues = new bool[(int)BasicType.String];
            for (int i = 0; i < validTypes.Length; i++)
            {
                validTypes[i] = true;
                nullvalues[i] = false;
                nonnullvalues[i] = false;
            }

            foreach (string? s in strings)
            {
                for (int i = 0; i < validTypes.Length; i++)
                {
                    if (validTypes[i])
                    {
                        BasicType bt = (BasicType)(i + 1);
                        if (TryParse(s, out object? nulltest, bt.FromBasicType(true)))
                        {
                            
                            if (nulltest == null)
                            {
                                nullvalues[i] = true;
                            }
                            else
                            {
                                if (bt == BasicType.ByteArray && s!.Length % 4 != 0)
                                {
                                    validTypes[i] = false;
                                }
                                else
                                {
                                    nonnullvalues[i] = true;
                                }
                            }
                        }
                        else
                        {
                            validTypes[i] = false;
                        }
                    }
                }
            }
            for (int i = 0; i < validTypes.Length; i++)
            {
                if(validTypes[i] && nonnullvalues[i])
                {
                    nullable = nullvalues[i];
                    return (BasicType)(i + 1);
                }
            }
            for (int i = 0; i < validTypes.Length; i++)
            {
                if (validTypes[i] && nullvalues[i])
                {
                    nullable = true;
                    return BasicType.None;
                }
            }
            nullable = false;
            return BasicType.None;
        }
        public enum BasicType
        {
            None      = 0,
            Boolean   = 1,
            Byte      = 2,
            Sbyte     = 3,
            UShort    = 4,
            Short     = 5,
            UInt      = 6,
            Int       = 7,
            ULong     = 8,
            Long      = 9,
            Single    = 10,
            Double    = 11,
            Decimal   = 12,
            Char      = 13,
            Guid      = 14,
            TimeSpan  = 15,
            DateTime  = 16,
            ByteArray = 17,
            String    = 18,
        }
        public static Type FromBasicType(this BasicType bt, bool nullable) => bt switch
        {
            BasicType.None => typeof(object),
            BasicType.Boolean => nullable ? typeof(bool?) : typeof(bool),
            BasicType.Byte => nullable ? typeof(byte?) : typeof(byte),
            BasicType.Sbyte => nullable ? typeof(sbyte?) : typeof(sbyte),
            BasicType.UShort => nullable ? typeof(ushort?) : typeof(ushort),
            BasicType.Short => nullable ? typeof(short?) : typeof(short),
            BasicType.UInt => nullable ? typeof(uint?) : typeof(uint),
            BasicType.Int => nullable ? typeof(int?) : typeof(int),
            BasicType.ULong => nullable ? typeof(ulong?) : typeof(ulong),
            BasicType.Long => nullable ? typeof(long?):typeof(long),
            BasicType.Single => nullable ? typeof(float?):typeof(float),
            BasicType.Double => nullable ? typeof(double?):typeof(double),
            BasicType.Decimal => nullable ? typeof(decimal?):typeof(decimal),
            BasicType.Char => nullable ? typeof(char?):typeof(char),
            BasicType.Guid => nullable ? typeof(Guid?):typeof(Guid),
            BasicType.TimeSpan => nullable ? typeof(TimeSpan?):typeof(TimeSpan),
            BasicType.DateTime => nullable ? typeof(DateTime?): typeof(DateTime),
            BasicType.ByteArray => typeof(byte[]),
            BasicType.String => typeof(string),
            _ => throw new NotImplementedException()
        };
        public static string ToString(this BasicType bt, bool nullable) => bt switch
        {
            BasicType.None => "object" + (nullable ?"?":""),
            BasicType.Boolean => "bool" + (nullable ? "?" : ""),
            BasicType.Byte => "byte" + (nullable ? "?" : ""),
            BasicType.Sbyte => "sbyte" + (nullable ? "?" : ""),
            BasicType.UShort => "ushort" + (nullable ? "?" : ""),
            BasicType.Short => "short" + (nullable ? "?" : ""),
            BasicType.UInt => "uint" + (nullable ? "?" : ""),
            BasicType.Int => "int" + (nullable ? "?" : ""),
            BasicType.ULong => "ulong" + (nullable ? "?" : ""),
            BasicType.Long => "long" + (nullable ? "?" : ""),
            BasicType.Single => "float" + (nullable ? "?" : ""),
            BasicType.Double => "double" + (nullable ? "?" : ""),
            BasicType.Decimal => "decimal" + (nullable ? "?" : ""),
            BasicType.Char => "char" + (nullable ? "?" : ""),
            BasicType.Guid => "Guid" + (nullable ? "?" : ""),
            BasicType.TimeSpan => "TimeSpan" + (nullable ? "?" : ""),
            BasicType.DateTime => "DateTime" + (nullable ? "?" : ""),
            BasicType.ByteArray => "byte[]" + (nullable ? "?" : ""),
            BasicType.String => "string" + (nullable ? "?" : ""),
            _ => throw new NotImplementedException()
        };
        #endregion
        public static bool IsNullable(ParameterInfo parameter)
        {
            if (parameter.ParameterType.IsValueType) { return Nullable.GetUnderlyingType(parameter.ParameterType) != null; }
            foreach(CustomAttributeData customAttribute in parameter.CustomAttributes)
            {
                if(customAttribute.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute")
                {
                    return (byte?)customAttribute.ConstructorArguments[0].Value == 2;
                }
            }

            for (MemberInfo? type = parameter.Member; type != null; type = type.DeclaringType)
            {
                foreach(CustomAttributeData customAttribute in type.CustomAttributes)
                {
                    if(customAttribute.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute")
                    {
                        return (byte?)customAttribute.ConstructorArguments[0].Value == 2;
                    }
                }
            }

            return false;
        }
    }
}
