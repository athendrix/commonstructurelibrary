using CSL.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSL.Data
{
    public class DataStore
    {
        private readonly Dictionary<string, string?> innerData;
        private readonly List<string> OrdinalValues;
        private readonly bool immutable;
        #region Constructors
        public DataStore()
        {
            innerData = new Dictionary<string, string?>();
            OrdinalValues = new List<string>();
            immutable = false;
        }

        public DataStore(IEnumerable<KeyValuePair<string, string>> input, bool immutable = false)
        {
            this.immutable = immutable;
            innerData = new Dictionary<string, string?>();
            OrdinalValues = new List<string>();
            
            foreach(KeyValuePair<string,string> entry in input)
            {
                innerData.Add(entry.Key, entry.Value);
                OrdinalValues.Add(entry.Key);
            }
        }

        public DataStore(string[] keys, string?[] values, bool immutable = false)
        {
            if(keys.Length != values.Length)
            {
                throw new ArgumentException("keys length and values length must be the same!");
            }
            this.immutable = immutable;
            innerData = new Dictionary<string, string?>();
            OrdinalValues = new List<string>();
            for(int i = 0; i < keys.Length; i++)
            {
                innerData.Add(keys[i], values[i]);
                OrdinalValues.Add(keys[i]);
            }
        }
        #endregion
        #region Mutable Functions
        public void Add(string key, string value)
        {
            if (immutable) { throw new InvalidOperationException("DataStore is immutable!"); }
            innerData.Add(key, value);
            OrdinalValues.Add(key);
        }
        public void Set(string key, string value)
        {
            if (immutable) { throw new InvalidOperationException("DataStore is immutable!"); }
            innerData[key] = value;
        }
        #endregion
        #region Base Types
        public string? GetString(string key)
        {
            if (innerData.ContainsKey(key))
            {
                return innerData[key];
            }
            else
            {
                return null;
            }
        }
        public T? Get<T>(string key)
        {
            string? value = GetString(key);
            if (Generics.TryParse(value, out T? result))
            {
                return result;
            }
            throw new FormatException("Failed to parse key \"" + key + "\" with value \"" + value + "\" to " + typeof(T?).Name + ".");
        }
        public int GetInt(string key) => Get<int>(key);
        public long GetLong(string key) => Get<long>(key);
        public bool GetBool(string key) => Get<bool>(key);
        public DateTime GetDateTime(string key) => Get<DateTime>(key);
        public byte[]? GetByteArray(string key) => Get<byte[]>(key);
        #endregion
        #region AdvancedTypes
        public T?[]? GetArray<T>(string key)
        {
            if (typeof(T) == typeof(byte))
            {
                return Get<byte[]>(key) as T[];
            }
            string? strvalue = GetString(key);
            if (strvalue == null)
            {
                return null;
            }
            string[] SplitArray = strvalue.Split(',');
            T?[] toReturn = new T[SplitArray.Length];
            for (int i = 0; i < toReturn.Length; i++)
            {
                if (Generics.TryParse<T>(SplitArray[i].Trim(), out T? result))
                {
                    toReturn[i] = result;
                }
                else
                {
                    throw new FormatException("Failed to parse key \"" + key + "\" with value \"" + strvalue + "\" to Array of " + typeof(T).Name + ".");
                }
            }
            return toReturn;
        }
        public Dictionary<T, U?>? GetDictionary<T, U>(string keyskey, string valueskey)
        {
            T?[]? keys = GetArray<T>(keyskey);
            U?[]? values = GetArray<U>(valueskey);
            if (keys == null && values == null)
            {
                return null;
            }
            if (keys == null) { throw new FormatException("Failed to parse \"" + keyskey + "\" as keys to " + typeof(Dictionary<T,U>).Name + "."); }
            if (values == null) { throw new FormatException("Failed to parse \"" + valueskey + "\" as values to " + typeof(Dictionary<T, U>).Name + "."); }
            if (keys.Length != values.Length) { throw new FormatException("Failed to parse \"" + keyskey + "\" and \"" + valueskey + "\" to " + typeof(Dictionary<T, U>).Name + ". Lengths differ."); }
            Dictionary<T, U?> toReturn = new Dictionary<T, U?>();
            for (int i = 0; i < keys.Length; i++)
            {
                T? key = keys[i];
                if (key == null) { continue; }
                toReturn.Add(key, values[i]);//throws exception on duplicate keys
            }
            return toReturn;
        }
        #endregion
    }

}
