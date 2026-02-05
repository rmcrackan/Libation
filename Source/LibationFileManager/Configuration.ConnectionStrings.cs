using System;
using System.ComponentModel;

namespace LibationFileManager;

public partial class Configuration
{
	[Description("Connection string for Postgresql")]
	public string? PostgresqlConnectionString
	{
		get => GetString(Environment.GetEnvironmentVariable("LIBATION_CONNECTION_STRING"));
		set => SetString(value);
	}
}
