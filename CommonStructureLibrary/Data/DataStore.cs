using CSL.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSL.Data
{
    public class DataStore
    {
        private readonly Dictionary<string, string> innerData;
        private readonly bool immutable;
        private readonly bool caseInsensitiveLookup;
        #region Constructors
        public DataStore(bool caseInsensitiveLookup = false)
        {
            innerData = new Dictionary<string, string>();
            immutable = false;
            this.caseInsensitiveLookup = caseInsensitiveLookup;
        }
        public DataStore(DataStore other, bool immutable)
        {
            this.immutable = false;
            this.caseInsensitiveLookup = other.caseInsensitiveLookup;
            innerData = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in other.innerData)
            {
                Add(entry.Key, entry.Value);
            }
            this.immutable = immutable;
        }
        public DataStore(IEnumerable<KeyValuePair<string, string?>> input, bool immutable = false, bool caseInsensitiveLookup = false)
        {
            this.immutable = false;
            this.caseInsensitiveLookup = caseInsensitiveLookup;
            innerData = new Dictionary<string, string>();
            
            foreach(KeyValuePair<string,string?> entry in input)
            {
                Add(entry.Key, entry.Value);
            }
            this.immutable = immutable;
        }
        public DataStore(string[] keys, string?[] values, bool immutable = false, bool caseInsensitiveLookup = false)
        {
            if(keys.Length != values.Length)
            {
                throw new ArgumentException("keys length and values length must be the same!");
            }
            this.immutable = false;
            this.caseInsensitiveLookup = caseInsensitiveLookup;
            innerData = new Dictionary<string, string>();
            for(int i = 0; i < keys.Length; i++)
            {
                Add(keys[i], values[i]);
            }
            this.immutable = immutable;
        }
        #endregion
        public string? this[string key]
        {
            get => GetString(key);
            set => Set(key, value);
        }
        #region Mutable Functions
        public void Add(string key, string? value)
        {
            if (immutable) { throw new InvalidOperationException("DataStore is immutable!"); }
            if (value != null)
            {
                innerData.Add(caseInsensitiveLookup ? key.ToUpper() : key, value);
            }
        }
        public void Set(string key, string? value)
        {
            if (immutable) { throw new InvalidOperationException("DataStore is immutable!"); }
            if(value == null)
            {
                if(innerData.ContainsKey(caseInsensitiveLookup ? key.ToUpper() : key))
                {
                    innerData.Remove(caseInsensitiveLookup ? key.ToUpper() : key);
                }
            }
            else
            {
                innerData[caseInsensitiveLookup ? key.ToUpper() : key] = value;
            }
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
        public int? GetInt(string key) => Get<int?>(key);
        public long? GetLong(string key) => Get<long?>(key);
        public bool? GetBool(string key) => Get<bool?>(key);
        public DateTime? GetDateTime(string key) => Get<DateTime?>(key);
        public byte[]? GetByteArray(string key) => Get<byte[]>(key);
        #endregion
        #region AdvancedTypes
        public T?[]? GetArray<T>(string key)
        {
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
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
