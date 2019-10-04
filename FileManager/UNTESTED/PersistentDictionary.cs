using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FileManager
{
    public class PersistentDictionary
    {
        public string Filepath { get; }

        // forgiving -- doesn't drop settings. old entries will continue to be persisted even if not publicly visible
        private Dictionary<string, string> settingsDic { get; }

        public string this[string key]
        {
            get => settingsDic[key];
            set
            {
                if (settingsDic.ContainsKey(key) && settingsDic[key] == value)
                    return;

                settingsDic[key] = value;

                // auto-save to file
                save();
            }
        }

        public PersistentDictionary(string filepath)
        {
            Filepath = filepath;

            // not found. create blank file
            if (!File.Exists(Filepath))
            {
                File.WriteAllText(Filepath, "{}");

                // give system time to create file before first use
                System.Threading.Thread.Sleep(100);
            }

            settingsDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Filepath));
        }

        public IEnumerable<string> Keys => settingsDic.Keys.Cast<string>();

        public void AddKeys(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return;

            foreach (var key in keys)
                settingsDic.Add(key, null);
            save();
        }

        private object locker { get; } = new object();
        private void save()
        {
            lock (locker)
                File.WriteAllText(Filepath, JsonConvert.SerializeObject(settingsDic, Formatting.Indented));
        }

        public static IEnumerable<System.Reflection.PropertyInfo> GetPropertiesToPersist(Type type)
            => type
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p =>
                // string properties only
                p.PropertyType == typeof(string)
                // exclude indexer
                && p.GetIndexParameters().Length == 0
                // exclude read-only, write-only
                && p.GetGetMethod(false) != null
                && p.GetSetMethod(false) != null
            ).ToList();
    }
}
