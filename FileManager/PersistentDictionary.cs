using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileManager
{
    public class PersistentDictionary
    {
        public string Filepath { get; }
        public bool IsReadOnly { get; }

        // optimize for strings. expectation is most settings will be strings and a rare exception will be something else
        private Dictionary<string, string> stringCache { get; } = new Dictionary<string, string>();
        private Dictionary<string, object> objectCache { get; } = new Dictionary<string, object>();

        public PersistentDictionary(string filepath, bool isReadOnly = false)
        {
            Filepath = filepath;
            IsReadOnly = isReadOnly;

            if (File.Exists(Filepath))
                return;

            // will create any missing directories, incl subdirectories. if all already exist: no action
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            if (IsReadOnly)
                return;

            File.WriteAllText(Filepath, "{}");
            System.Threading.Thread.Sleep(100);
        }

        public string GetString(string propertyName)
        {
            if (!stringCache.ContainsKey(propertyName))
            {
                var jObject = readFile();
                if (!jObject.ContainsKey(propertyName))
                    return null;
                stringCache[propertyName] = jObject[propertyName].Value<string>();
            }

            return stringCache[propertyName];
        }

        public T Get<T>(string propertyName)
        {
            var obj = GetObject(propertyName);
            if (obj is null) return default;
            if (obj is JToken jToken) return jToken.Value<T>();
            return (T)obj;
        }

        public object GetObject(string propertyName)
        {
            if (!objectCache.ContainsKey(propertyName))
            {
                var jObject = readFile();
                if (!jObject.ContainsKey(propertyName))
                    return null;
                objectCache[propertyName] = jObject[propertyName].Value<object>();
            }

            return objectCache[propertyName];
        }

        public bool Exists(string propertyName) => readFile().ContainsKey(propertyName);

        private object locker { get; } = new object();
        public void Set(string propertyName, string newValue)
        {
            // only do this check in string cache, NOT object cache
            if (stringCache.ContainsKey(propertyName) && stringCache[propertyName] == newValue)
                return;

            // set cache
            stringCache[propertyName] = newValue;

            writeFile(propertyName, newValue);
        }

        public void Set(string propertyName, object newValue)
        {
            // set cache
            objectCache[propertyName] = newValue;

            var parsedNewValue = JToken.Parse(JsonConvert.SerializeObject(newValue));
            writeFile(propertyName, parsedNewValue);
        }

        private void writeFile(string propertyName, JToken newValue)
        {
            if (IsReadOnly)
                return;

            try
            {
                var str = newValue?.ToString();
                var formattedValue
                    = str is null ? "[null]"
                    : string.IsNullOrEmpty(str) ? "[empty]"
                    : string.IsNullOrWhiteSpace(str) ? $"[whitespace. Length={str.Length}]"
                    : str.Length > 100 ? $"[Length={str.Length}] {str[0..50]}...{str[^50..^0]}"
                    : str;
                Serilog.Log.Logger.Information($"Config changed. {propertyName}={formattedValue}");
            }
            catch { }

            // write new setting to file
            lock (locker)
            {
                var jObject = readFile();
                var startContents = JsonConvert.SerializeObject(jObject, Formatting.Indented);

                jObject[propertyName] = newValue;
                var endContents = JsonConvert.SerializeObject(jObject, Formatting.Indented);

                if (startContents != endContents)
                    File.WriteAllText(Filepath, endContents);
            }
        }

        // special case: no caching. no logging
        public void SetWithJsonPath(string jsonPath, string propertyName, string newValue)
        {
            if (IsReadOnly)
                return;

            lock (locker)
            {
                var jObject = readFile();
                var token = jObject.SelectToken(jsonPath);
                var oldValue = (string)token[propertyName];

                if (oldValue != newValue)
                {
                    token[propertyName] = newValue;
                    File.WriteAllText(Filepath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
                }
            }
        }

        private JObject readFile()
        {
            var settingsJsonContents = File.ReadAllText(Filepath);
            var jObject = JsonConvert.DeserializeObject<JObject>(settingsJsonContents);
            return jObject;
        }
    }
}
