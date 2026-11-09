using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationFileManager.Tests;

[TestClass]
public class EnvironmentConfigurationTests
{
	[TestMethod]
	public void wayland_session_is_detected_when_wayland_display_is_set()
	{
		var isWayland = Configuration.IsWaylandSession(varName => varName switch
		{
			"WAYLAND_DISPLAY" => "wayland-0",
			_ => null
		});

		Assert.IsTrue(isWayland);
	}

	[TestMethod]
	public void wayland_session_is_detected_when_xdg_session_type_is_wayland()
	{
		var isWayland = Configuration.IsWaylandSession(varName => varName switch
		{
			"XDG_SESSION_TYPE" => "wayland",
			_ => null
		});

		Assert.IsTrue(isWayland);
	}

	[TestMethod]
	public void wayland_session_is_not_detected_when_x11()
	{
		var isWayland = Configuration.IsWaylandSession(varName => varName switch
		{
			"XDG_SESSION_TYPE" => "x11",
			"DISPLAY" => ":0",
			_ => null
		});

		Assert.IsFalse(isWayland);
	}

	[TestMethod]
	public void wayland_session_is_not_detected_when_env_empty()
	{
		var isWayland = Configuration.IsWaylandSession(_ => null);

		Assert.IsFalse(isWayland);
	}
}
