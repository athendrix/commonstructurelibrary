using System;

namespace CSL.Helpers
{
    public static class Generics
    {
        public static bool TryParse<T>(string s, out T result)
        {
            bool toReturn;
            Type TType = typeof(T);
            if (TType == typeof(String))
            {
                result = (T)(object)s;
                return true;
            }
            if ((s == "" || s == null) && default(T) == null)
            {
                result = default;
                return true;
            }
            #region Signed Integers
            if (TType == typeof(SByte) || TType == typeof(SByte?))
            {
                toReturn = SByte.TryParse(s, out SByte Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(Int16) || TType == typeof(Int16?))
            {
                toReturn = Int16.TryParse(s, out Int16 Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(Int32) || TType == typeof(Int32?))
            {
                toReturn = Int32.TryParse(s, out Int32 Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(Int64) || TType == typeof(Int64?))
            {
                toReturn = Int64.TryParse(s, out Int64 Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            #endregion
            #region Unsigned Integers
            if (TType == typeof(Byte) || TType == typeof(Byte?))
            {
                toReturn = Byte.TryParse(s, out Byte Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(UInt16) || TType == typeof(UInt16?))
            {
                toReturn = UInt16.TryParse(s, out UInt16 Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(UInt32) || TType == typeof(UInt32?))
            {
                toReturn = UInt32.TryParse(s, out UInt32 Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(UInt64) || TType == typeof(UInt64?))
            {
                toReturn = UInt64.TryParse(s, out UInt64 Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            #endregion
            #region Floating Point and Decimal
            if (TType == typeof(Single) || TType == typeof(Single?))
            {
                toReturn = Single.TryParse(s, out Single Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(Double) || TType == typeof(Double?))
            {
                toReturn = Double.TryParse(s, out Double Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            if (TType == typeof(Decimal) || TType == typeof(Decimal?))
            {
                toReturn = Decimal.TryParse(s, out Decimal Tresult);
                result = (T)(object)Tresult;
                return toReturn;
            }
            #endregion
            #region Other
            if (TType == typeof(Boolean) || TType == typeof(Boolean?))
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
                toReturn = Boolean.TryParse(s, out Boolean Tresult);
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
            if (TType == typeof(Char) || TType == typeof(Char?))
            {
#if BRIDGE
                        toReturn = true;
                        char Tresult;
                        try
                        {
                            Tresult = Char.Parse(s);
                        }
                        catch(Exception)
                        {
                            toReturn = false;
                            Tresult = default;
                        }
#else
                toReturn = Char.TryParse(s, out Char Tresult);
#endif
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
#if BRIDGE
                            toReturn = true;
                            object Tresult;
                            try
                            {
                                Tresult = Enum.Parse(TType, s, true);
                            }
                            catch (Exception)
                            {
                                toReturn = false;
                                Tresult = default;
                            }
#else
                    toReturn = Enum.TryParse(TType, s, true, out object Tresult);
#endif
                    result = (T)Tresult;
                    return toReturn;
                }
                catch (Exception)
                {
                    result = default(T);
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
                    result = default(T);
                    return false;
                }
            }
            #endregion
            #region Serialization
            try
            {
                //Attribute[] attributes = Attribute.GetCustomAttributes(TType.Assembly, typeof(SerializableAttribute));
                //if (attributes.Length > 0)
                //{
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);
                return true;
                //}
            }
            catch { }
            #endregion
            result = default(T);
            return false;
        }
        public static string ToString<T>(T input)
        {
            Type TType = typeof(T);
            if (TType == typeof(String))
            {
                return (String)(object)input;
            }
            if (input == null)
            {
                return null;
            }
            #region Signed Integers
            if (TType == typeof(SByte) || TType == typeof(SByte?))
            {
                SByte temp = (SByte)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(Int16) || TType == typeof(Int16?))
            {
                Int16 temp = (Int16)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(Int32) || TType == typeof(Int32?))
            {
                Int32 temp = (Int32)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(Int64) || TType == typeof(Int64?))
            {
                Int64 temp = (Int64)(object)input;
                return temp.ToString();
            }
            #endregion
            #region Unsigned Integers
            if (TType == typeof(Byte) || TType == typeof(Byte?))
            {
                Byte temp = (Byte)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(UInt16) || TType == typeof(UInt16?))
            {
                UInt16 temp = (UInt16)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(UInt32) || TType == typeof(UInt32?))
            {
                UInt32 temp = (UInt32)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(UInt64) || TType == typeof(UInt64?))
            {
                UInt64 temp = (UInt64)(object)input;
                return temp.ToString();
            }
            #endregion
            #region Floating Point and Decimal
            if (TType == typeof(Single) || TType == typeof(Single?))
            {
                Single temp = (Single)(object)input;
                return temp.ToString("R");
            }
            if (TType == typeof(Double) || TType == typeof(Double?))
            {
                Double temp = (Double)(object)input;
                return temp.ToString("R");
            }
            if (TType == typeof(Decimal) || TType == typeof(Decimal?))
            {
                Decimal temp = (Decimal)(object)input;
                return temp.ToString();
            }
            #endregion
            #region Other
            if (TType == typeof(Boolean) || TType == typeof(Boolean?))
            {
                Boolean temp = (Boolean)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(Guid) || TType == typeof(Guid?))
            {
                Guid temp = (Guid)(object)input;
                return temp.ToString("D");
            }
            if (TType == typeof(DateTime) || TType == typeof(DateTime?))
            {
                DateTime temp = (DateTime)(object)input;
                temp = temp.ToUniversalTime();
                return temp.ToString("O");
            }
            if (TType == typeof(Char) || TType == typeof(Char?))
            {
                Char temp = (Char)(object)input;
                return temp.ToString();
            }
            if (TType == typeof(TimeSpan) || TType == typeof(TimeSpan?))
            {
                TimeSpan temp = (TimeSpan)(object)input;
                return temp.ToString("G");
            }
            if (TType.IsEnum)
            {
                return Enum.GetName(TType, input);
            }
            if (TType == typeof(byte[]))
            {
                byte[] temp = (byte[])(object)input;
                return WebBase64.Encode(temp);
            }
            #endregion
            #region Serialization
            try
            {
                //object[] attributes = TType.GetCustomAttributes(typeof(SerializableAttribute), true);
                //if (attributes.Length > 0)
                //{
                return Newtonsoft.Json.JsonConvert.SerializeObject(input);
                //}
            }
            catch { }
            #endregion
            return input.ToString();
        }
    }
}
