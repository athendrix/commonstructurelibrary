using System;
using System.Collections;
using System.Collections.Generic;

namespace CSL.Data
{
    public static class Env
    {

        private static readonly Dictionary<string, string> Defaults = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> RawEnv = new Dictionary<string, string>();
        private static Dictionary<string, string?> GetEnv()
        {
            Dictionary<string,string?> toReturn = new Dictionary<string,string?>();
            foreach(string key in Defaults.Keys)
            {
                toReturn.Add(key, toReturn[key]);
            }
            foreach(string key in RawEnv.Keys)
            {
                toReturn[key] = toReturn[key];//Overwrite any defaults
            }
            return toReturn;
        }
        private static DataStore<string>? _DS = null;
        static Env() => RefreshEnv();
        public static void RefreshEnv()
        {
            _DS = null;
            RawEnv.Clear();
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                RawEnv.Add(entry.Key.ToString().ToUpper(), entry.Value.ToString());
            }
        }
        public static void SetDefault(string key, string value) => Defaults[key.ToUpper()] = value;

        public static DataStore<string> Vars => _DS ??= new DataStore<string>(GetEnv(), true);
    }
}
