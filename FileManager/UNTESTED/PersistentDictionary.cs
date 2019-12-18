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

        // optimize for strings. expectation is most settings will be strings and a rare exception will be something else
        private Dictionary<string, string> stringCache { get; } = new Dictionary<string, string>();
        private Dictionary<string, object> objectCache { get; } = new Dictionary<string, object>();

        public PersistentDictionary(string filepath)
        {
            Filepath = filepath;

            if (File.Exists(Filepath))
                return;

            // will create any missing directories, incl subdirectories. if all already exist: no action
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            File.WriteAllText(Filepath, "{}");
            System.Threading.Thread.Sleep(100);
        }

        public string GetString(string propertyName)
        {
            if (!stringCache.ContainsKey(propertyName))
            {
                var jObject = readFile();
                stringCache[propertyName] = jObject.ContainsKey(propertyName) ? jObject[propertyName].Value<string>() : null;
            }

            return stringCache[propertyName];
        }

        public T Get<T>(string propertyName) where T : class
            => GetObject(propertyName) is T obj ? obj : default;

        public object GetObject(string propertyName)
        {
            if (!objectCache.ContainsKey(propertyName))
            {
                var jObject = readFile();
                objectCache[propertyName] = jObject.ContainsKey(propertyName) ? jObject[propertyName].Value<object>() : null;
            }

            return objectCache[propertyName];
        }

        private object locker { get; } = new object();
        public void Set(string propertyName, string newValue)
        {
            // only do this check in string cache, NOT object cache
            if (stringCache[propertyName] == newValue)
                return;

            // set cache
            stringCache[propertyName] = newValue;

            // set in file
            lock (locker)
            {
                var jObject = readFile();
                jObject[propertyName] = newValue;
                File.WriteAllText(Filepath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
            }
        }

        public void Set(string propertyName, object newValue)
        {
            // set cache
            objectCache[propertyName] = newValue;

            // set in file
            lock (locker)
            {
                var jObject = readFile();
                jObject[propertyName] = JToken.Parse(JsonConvert.SerializeObject(newValue));
                File.WriteAllText(Filepath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
            }
        }

        public void SetWithJsonPath(string jsonPath, string propertyName, string newValue)
        {
            lock (locker)
            {
                var jObject = readFile();
                var token = jObject.SelectToken(jsonPath);
                var debug_oldValue = (string)token[propertyName];
                token[propertyName] = newValue;
                File.WriteAllText(Filepath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
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
