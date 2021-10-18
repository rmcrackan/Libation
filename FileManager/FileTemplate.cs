using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;

namespace FileManager
{
    /// <summary>Get valid filename. Advanced features incl. parameterized template</summary>
    public class FileTemplate
    {
        /// <summary>Proposed full file path. May contain optional html-styled template tags. Eg: &lt;name&gt;</summary>
        public string Template { get; }

        /// <param name="template">Proposed file name with optional html-styled template tags.</param>
        public FileTemplate(string template) => Template = ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));

        /// <summary>Optional step 1: Replace html-styled template tags with parameters. Eg {"name", "Bill Gates"} => /&lt;name&gt;/ => /Bill Gates/</summary>
        public Dictionary<string, string> ParameterReplacements { get; } = new Dictionary<string, string>();

        /// <summary>Convenience method</summary>
        public void AddParameterReplacement(string key ,string value) => ParameterReplacements.Add(key, value);

        /// <summary>If set, truncate each parameter replacement to this many characters. Default 50</summary>
        public int? ParameterMaxSize { get; set; } = 50;

        /// <summary>Optional step 2: Replace all illegal characters with this. Default=<see cref="string.Empty"/></summary>
        public string IllegalCharacterReplacements { get; set; }

        /// <summary>Generate a valid path for this file or directory</summary>
        public string GetFilename()
        {
            var filename = Template;

            foreach (var r in ParameterReplacements)
                filename = filename.Replace($"<{formatKey(r.Key)}>", formatValue(r.Value));

            return FileUtility.GetValidFilename(filename, IllegalCharacterReplacements);
        }

        private string formatKey(string key)
            => key
            .Replace("<", "")
            .Replace(">", "");

        private string formatValue(string value)
            => ParameterMaxSize.HasValue && ParameterMaxSize.Value > 0
            ? value?.Truncate(ParameterMaxSize.Value)
            : value;
    }

}
