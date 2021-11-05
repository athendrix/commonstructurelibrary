using System;

namespace CSL.Helpers
{
    public static class Generics
    {
        public static bool TryParse<T>(string? s, out T? result)
        {
            bool toReturn;
            Type TType = typeof(T?);
            #region Special Cases
            if (TType == typeof(string))
            {
                result = (T?)(object?)s;
                return true;
            }
            if(s == "" && TType == typeof(byte[]))
            {
                result = (T)(object)new byte[0];
                return true;
            }
            if (s == "" && (TType == typeof(Guid) || TType == typeof(Guid?)))
            {
                result = (T)(object)Guid.Empty;
                return true;
            }
            if (s is "" or null)
            {
                result = default(T?);
                return default(T?) == null;
            }
            #endregion
            #region Signed Integers
            if (TType == typeof(sbyte) || TType == typeof(sbyte?))
            {
                toReturn = sbyte.TryParse(s, out sbyte Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(short) || TType == typeof(short?))
            {
                toReturn = short.TryParse(s, out short Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(int) || TType == typeof(int?))
            {
                toReturn = int.TryParse(s, out int Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(long) || TType == typeof(long?))
            {
                toReturn = long.TryParse(s, out long Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            #endregion
            #region Unsigned Integers
            if (TType == typeof(byte) || TType == typeof(byte?))
            {
                toReturn = byte.TryParse(s, out byte Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(ushort) || TType == typeof(ushort?))
            {
                toReturn = ushort.TryParse(s, out ushort Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(uint) || TType == typeof(uint?))
            {
                toReturn = uint.TryParse(s, out uint Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(ulong) || TType == typeof(ulong?))
            {
                toReturn = ulong.TryParse(s, out ulong Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            #endregion
            #region Floating Point and Decimal
            if (TType == typeof(float) || TType == typeof(float?))
            {
                toReturn = float.TryParse(s, out float Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(double) || TType == typeof(double?))
            {
                toReturn = double.TryParse(s, out double Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(decimal) || TType == typeof(decimal?))
            {
                toReturn = decimal.TryParse(s, out decimal Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            #endregion
            #region Other
            if (TType == typeof(bool) || TType == typeof(bool?))
            {
                string uppers = s.ToUpper();
                if (s == "0" || uppers == "F" || uppers == "N" || uppers == "NO")
                {
                    result = (T)(object)false;
                    return true;
                }
                if (s == "1" || uppers == "T" || uppers == "Y" || uppers == "YES")
                {
                    result = (T)(object)true;
                    return true;
                }
                toReturn = bool.TryParse(s, out bool Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(Guid) || TType == typeof(Guid?))
            {
                toReturn = Guid.TryParse(s, out Guid Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(DateTime) || TType == typeof(DateTime?))
            {
                toReturn = DateTime.TryParse(s, out DateTime Tresult);
                Tresult = Tresult.ToUniversalTime();
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(char) || TType == typeof(char?))
            {
                toReturn = char.TryParse(s, out char Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(TimeSpan) || TType == typeof(TimeSpan?))
            {
                toReturn = TimeSpan.TryParse(s, out TimeSpan Tresult);
                result = (T)(object)Tresult;
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
                        Tresult = default(T?);
                    }
                    result = (T?)Tresult;
                    return toReturn;
                }
                catch (Exception)
                {
                    result = default(T?);
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
                    result = (T)(object)Convert.FromBase64String(temps);
                    return true;
                }
                catch (Exception)
                {
                    result = default(T?);
                    return false;
                }
            }
            #endregion
            #region Serialization
            try
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);
                return true;
            }
            catch { }
            #endregion
            result = default(T);
            return false;
        }
        public static string? ToString<T>(T? input)
        {
            Type TType = typeof(T);
            if (TType == typeof(string))
            {
                return (string?)(object?)input;
            }
            if (input == null)
            {
                return null;
            }
            #region Signed Integers
            if (TType == typeof(sbyte) || TType == typeof(sbyte?))
            {
                return((sbyte)(object)input).ToString();
            }
            if (TType == typeof(short) || TType == typeof(short?))
            {
                return ((short)(object)input).ToString();
            }
            if (TType == typeof(int) || TType == typeof(int?))
            {
                return ((int)(object)input).ToString();
            }
            if (TType == typeof(long) || TType == typeof(long?))
            {
                return ((long)(object)input).ToString();
            }
            #endregion
            #region Unsigned Integers
            if (TType == typeof(byte) || TType == typeof(byte?))
            {
                return ((byte)(object)input).ToString();
            }
            if (TType == typeof(ushort) || TType == typeof(ushort?))
            {
                return ((ushort)(object)input).ToString();
            }
            if (TType == typeof(uint) || TType == typeof(uint?))
            {
                return ((uint)(object)input).ToString();
            }
            if (TType == typeof(UInt64) || TType == typeof(UInt64?))
            {
                return ((ulong)(object)input).ToString();
            }
            #endregion
            #region Floating Point and Decimal
            if (TType == typeof(float) || TType == typeof(float?))
            {
                return ((float)(object)input).ToString("R");
            }
            if (TType == typeof(double) || TType == typeof(double?))
            {
                return ((double)(object)input).ToString("R");
            }
            if (TType == typeof(Decimal) || TType == typeof(Decimal?))
            {
                return ((decimal)(object)input).ToString();
            }
            #endregion
            #region Other
            if (TType == typeof(bool) || TType == typeof(bool?))
            {
                return((bool)(object)input).ToString();
            }
            if (TType == typeof(Guid) || TType == typeof(Guid?))
            {
                return ((Guid)(object)input).ToString();
            }
            if (TType == typeof(DateTime) || TType == typeof(DateTime?))
            {
                return ((DateTime)(object)input).ToUniversalTime().ToString("O");
            }
            if (TType == typeof(char) || TType == typeof(char?))
            {
                return ((char)(object)input).ToString();
            }
            if (TType == typeof(TimeSpan) || TType == typeof(TimeSpan?))
            {
                return ((TimeSpan)(object)input).ToString("G");
            }
            if (TType.IsEnum)
            {
                return Enum.GetName(TType, input);
            }
            if (TType == typeof(byte[]))
            {
                return ((byte[])(object)input).EncodeToWebBase64();
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
    }
}
