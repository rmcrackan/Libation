using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;

namespace FileManager
{
    /// <summary>Get valid filename. Advanced features incl. parameterized template</summary>
    public class FileNamingTemplate
    {
        /// <summary>Proposed full file path. May contain optional html-styled template tags. Eg: &lt;name&gt;</summary>
        public string Template { get; }

        /// <param name="template">Proposed file name with optional html-styled template tags.</param>
        public FileNamingTemplate(string template) => Template = ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));

        /// <summary>Optional step 1: Replace html-styled template tags with parameters. Eg {"name", "Bill Gates"} => /&lt;name&gt;/ => /Bill Gates/</summary>
        public Dictionary<string, object> ParameterReplacements { get; } = new Dictionary<string, object>();

        /// <summary>Convenience method</summary>
        public void AddParameterReplacement(string key, object value)
            // using .Add() instead of "[key] = value" will make unintended overwriting throw exception
            => ParameterReplacements.Add(key, value);

        /// <summary>If set, truncate each parameter replacement to this many characters. Default 50</summary>
        public int? ParameterMaxSize { get; set; } = 50;

        /// <summary>Optional step 2: Replace all illegal characters with this. Default=<see cref="string.Empty"/></summary>
        public string IllegalCharacterReplacements { get; set; }

        /// <summary>Generate a valid path for this file or directory</summary>
        public string GetFilePath(bool returnFirstExisting = false)
        {
            var filename = Template;

            foreach (var r in ParameterReplacements)
                filename = filename.Replace($"<{formatKey(r.Key)}>", formatValue(r.Value));

            return FileUtility.GetValidFilename(filename, IllegalCharacterReplacements, returnFirstExisting);
        }

        private static string formatKey(string key)
            => key
            .Replace("<", "")
            .Replace(">", "");

        private string formatValue(object value)
        {
            if (value is null)
                return "";

            // Other illegal characters will be taken care of later. Must take care of slashes now so params can't introduce new folders.
            // Esp important for file templates.
            var val = value
                .ToString()
                .Replace($"{System.IO.Path.DirectorySeparatorChar}", IllegalCharacterReplacements)
                .Replace($"{System.IO.Path.AltDirectorySeparatorChar}", IllegalCharacterReplacements);
            return 
                ParameterMaxSize.HasValue && ParameterMaxSize.Value > 0
                ? val.Truncate(ParameterMaxSize.Value)
                : val;
        }
    }
}
