using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LibationUiBase.Tests;

[TestClass]
public class AudibleAppLinkMessageTests
{
	// Link targets copied from Audible's Android sign-in pages (amazon.com and the pre-amazon audible.com page).
	[TestMethod]
	public void username_link_points_to_pre_amazon_locale()
		=> StringAssert.Contains(AudibleAppLinkMessage.ForNavigation(new Uri("audible://view?section=authportal&pageType=private")), "pre-amazon - us");

	[TestMethod]
	public void amazon_link_points_away_from_pre_amazon_locale()
		=> StringAssert.Contains(AudibleAppLinkMessage.ForNavigation(new Uri("audible://view?section=authportal&pageType=public")), "Amazon account");

	[TestMethod]
	public void web_navigation_is_left_alone()
		=> Assert.IsNull(AudibleAppLinkMessage.ForNavigation(new Uri("https://www.amazon.com/ap/signin?pageId=amzn_audible_android_aui_v2_dark_us")));
}
