using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dinah.Core;
using Dinah.Core.Logging;
using FileManager;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace LibationFileManager
{
    public partial class Configuration
    {
        private IConfigurationRoot configuration;

        public void ConfigureLogging()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile(SettingsFilePath, optional: false, reloadOnChange: true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.ByTransforming<LongPath>(lp => lp.Path)
                .CreateLogger();
        }

        [Description("The importance of a log event")]
        public LogEventLevel LogLevel
        {
            get
            {
                var logLevelStr = persistentDictionary.GetStringFromJsonPath("Serilog", "MinimumLevel");
                return Enum.TryParse<LogEventLevel>(logLevelStr, out var logLevelEnum) ? logLevelEnum : LogEventLevel.Information;
            }
            set
            {
				OnPropertyChanging(nameof(LogLevel), LogLevel, value);
                var valueWasChanged = persistentDictionary.SetWithJsonPath("Serilog", "MinimumLevel", value.ToString());
                if (!valueWasChanged)
                {
                    Log.Logger.Debug("LogLevel.set attempt. No change");
                    return;
                }

                configuration.Reload();

				OnPropertyChanged(nameof(LogLevel), value);

				Log.Logger.Information("Updated LogLevel MinimumLevel. {@DebugInfo}", new
                {
                    LogLevel_Verbose_Enabled = Log.Logger.IsVerboseEnabled(),
                    LogLevel_Debug_Enabled = Log.Logger.IsDebugEnabled(),
                    LogLevel_Information_Enabled = Log.Logger.IsInformationEnabled(),
                    LogLevel_Warning_Enabled = Log.Logger.IsWarningEnabled(),
                    LogLevel_Error_Enabled = Log.Logger.IsErrorEnabled(),
                    LogLevel_Fatal_Enabled = Log.Logger.IsFatalEnabled()
                });
            }
        }
    }
}
