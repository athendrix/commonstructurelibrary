using System;

namespace CSL.Helpers
{
    public static class Generics
    {
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
            if (TType == typeof(string))
            {
                result = s;
                return true;
            }
            if (s == "" && TType == typeof(byte[]))
            {
                result = new byte[0];
                return true;
            }
            if (s == "" && (TType == typeof(Guid) || TType == typeof(Guid?)))
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
                if (s == "0" || uppers == "F" || uppers == "N" || uppers == "NO")
                {
                    result = false;
                    return true;
                }
                if (s == "1" || uppers == "T" || uppers == "Y" || uppers == "YES")
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
    }
}
