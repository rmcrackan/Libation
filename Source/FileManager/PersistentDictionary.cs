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

            createNewFile();
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

        public T GetNonString<T>(string propertyName)
        {
            var obj = GetObject(propertyName);
            if (obj is null) return default;
            if (obj is JValue jValue) return jValue.Value<T>();
            if (obj is JObject jObject)  return jObject.ToObject<T>();
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

        public string GetStringFromJsonPath(string jsonPath, string propertyName) => GetStringFromJsonPath($"{jsonPath}.{propertyName}");
        public string GetStringFromJsonPath(string jsonPath)
        {
            if (!stringCache.ContainsKey(jsonPath))
            {
                try
                {
                    var jObject = readFile();
                    var token = jObject.SelectToken(jsonPath);
                    if (token is null)
                        return null;
                    stringCache[jsonPath] = (string)token;
                }
                catch
                {
                    return null;
                }
            }

            return stringCache[jsonPath];
        }

        public bool Exists(string propertyName) => readFile().ContainsKey(propertyName);

        private object locker { get; } = new object();
        public void SetString(string propertyName, string newValue)
        {
            // only do this check in string cache, NOT object cache
            if (stringCache.ContainsKey(propertyName) && stringCache[propertyName] == newValue)
                return;

            // set cache
            stringCache[propertyName] = newValue;

            writeFile(propertyName, newValue);
        }

        public void SetNonString(string propertyName, object newValue)
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

            // write new setting to file
            lock (locker)
            {
                var jObject = readFile();
                var startContents = JsonConvert.SerializeObject(jObject, Formatting.Indented);

                jObject[propertyName] = newValue;
                var endContents = JsonConvert.SerializeObject(jObject, Formatting.Indented);

                if (startContents == endContents)
                    return;
                
                File.WriteAllText(Filepath, endContents);
            }

            try
            {
                var str = formatValueForLog(newValue?.ToString());
                Serilog.Log.Logger.Information("Config changed. {@DebugInfo}", new { propertyName, newValue = str });
            }
            catch { }
        }

        /// <summary>WILL ONLY set if already present. WILL NOT create new</summary>
        /// <returns>Value was changed</returns>
        public bool SetWithJsonPath(string jsonPath, string propertyName, string newValue, bool suppressLogging = false)
        {
            if (IsReadOnly)
                return false;

            var path = $"{jsonPath}.{propertyName}";

            {
                // only do this check in string cache, NOT object cache
                if (stringCache.ContainsKey(path) && stringCache[path] == newValue)
                    return false;

                // set cache
                stringCache[path] = newValue;
            }

            try
            {
                lock (locker)
                {
                    var jObject = readFile();
                    var token = jObject.SelectToken(jsonPath);
                    if (token is null || token[propertyName] is null)
                        return false;

                    var oldValue = token.Value<string>(propertyName);
                    if (oldValue == newValue)
                        return false;

                    token[propertyName] = newValue;
                    File.WriteAllText(Filepath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
                }
            }
            catch (Exception exDebug)
            {
                return false;
            }

            if (!suppressLogging)
            {
                try
                {
                    var str = formatValueForLog(newValue?.ToString());
                    Serilog.Log.Logger.Information("Config changed. {@DebugInfo}", new { jsonPath, propertyName, newValue = str });
                }
                catch { }
            }

            return true;
        }

        private static string formatValueForLog(string value)
            => value is null ? "[null]"
            : string.IsNullOrEmpty(value) ? "[empty]"
            : string.IsNullOrWhiteSpace(value) ? $"[whitespace. Length={value.Length}]"
            : value.Length > 100 ? $"[Length={value.Length}] {value[0..50]}...{value[^50..^0]}"
            : value;

        private JObject readFile()
        {
            if (!File.Exists(Filepath))
            {
                var msg = "Unrecoverable error. Settings file cannot be found";
                var ex = new FileNotFoundException(msg, Filepath);
                Serilog.Log.Logger.Error(ex, msg);
                throw ex;
            }

            var settingsJsonContents = File.ReadAllText(Filepath);

            if (string.IsNullOrWhiteSpace(settingsJsonContents))
            {
                createNewFile();
                settingsJsonContents = File.ReadAllText(Filepath);
            }

            var jObject = JsonConvert.DeserializeObject<JObject>(settingsJsonContents);

            if (jObject is null)
            {
                var msg = "Unrecoverable error. Unable to read settings from Settings file";
                var ex = new NullReferenceException(msg);
                Serilog.Log.Logger.Error(ex, msg);
                throw ex;
            }

            return jObject;
        }

        private void createNewFile()
        {
            File.WriteAllText(Filepath, "{}");
            System.Threading.Thread.Sleep(100);
        }
    }
}
