namespace LibationUiBase.StatusIcons;

/// <summary>
/// The vector artwork and palette for the Liberate column's icons. These are the single source of
/// truth for both UIs; <see cref="StatusImageGenerator"/> rasterizes them.
/// </summary>
/// <remarks>
/// Path data is SVG path mini-language, and is filled using the even-odd rule so that nested
/// subpaths (the stoplight's lamp bezels, the error ring, the series square's border) cut holes.
/// </remarks>
internal static class LiberateIconGeometry
{
	/// <summary>Rounded body with three lamp bezels cut out of it. Natural size 46 x 100.</summary>
	public const string StoplightBody = """
		M0,12 A 12,12 0 0 1 12,0 H34 A 12,12 0 0 1 46,12 V88 A 12,12 0 0 1 34,100 H12 A 12,12 0 0 1 0,88 V12
		M20,8 H26 A 12,12 0 0 1 26,32 H20 A 12,12 0 0 1 20,8
		M20,38 H26 A 12,12 0 0 1 26,62 H20 A 12,12 0 0 1 20,38
		M20,68 H26 A 12,12 0 0 1 26,92 H20 A 12,12 0 0 1 20,68
		""";

	/// <summary>A PDF document glyph. Natural size 45 x 50.5.</summary>
	public const string PdfDocument = """
		M4,38.5 H3 A 3,3 0 0 1 0,35.5 V21.4 A 3,3 0 0 1 3,18.4 H4 V2 A 2,2 0 0 1 6,0 H30.5 L41,12 V18.4 A 3,3 0 0 1 45,21.4 V35.5 A 3,3 0 0 1 42,38.5 H41 V48.5 A 2,2 0 0 1 39,50.5 H6 A 2,2 0 0 1 4,48.5
		M6,38.5 H39 V48.5 H6 V38.5
		M6,18.4 V2 H29 V12 A 1,1 0 0 0 30,13 H39 V18.4
		M 4.3179,36 c 0,0 0.122,-14.969 0.122,-14.969 1.469,-0.194 2.939,-0.388 4.5,-0.362 1.561,0.026 3.214,0.27 4.357,0.944 1.143,0.674 1.775,1.776 2.015,2.959 0.24,1.184 0.087,2.449 -0.5,3.52 -0.587,1.071 -1.607,1.949 -2.816,2.352 -1.209,0.403 -2.607,0.332 -4.005,0.26 0,0 -0.031,5.265 -0.031,5.265 0,0 -3.673,0.122 -3.673,0.122 0,0 0.031,-0.092 0.031,-0.092
		m 3.643,-12.428 c 0,0 0.031,4.286 0.031,4.286 0.735,0.051 1.47,0.102 2.107,-0.056 0.638,-0.158 1.178,-0.526 1.459,-1.122 0.281,-0.597 0.301,-1.423 0.01,-2.005 -0.291,-0.582 -0.893,-0.918 -1.546,-1.061 -0.653,-0.143 -1.357,-0.092 -1.709,-0.066 -0.352,0.026 -0.352,0.026 -0.352,0.026
		m 9.428,12.428 c 2.265,0.245 4.531,0.49 6.674,0.066 2.143,-0.424 4.163,-1.515 5.285,-3.081 1.122,-1.566 1.347,-3.607 1.27,-5.306 -0.076,-1.699 -0.454,-3.056 -1.454,-4.219 -1,-1.163 -2.622,-2.133 -4.704,-2.505 -2.082,-0.373 -4.623,-0.148 -7.164,0.076 0,0 0.092,14.969 0.092,14.969
		m 3.49,-12.398 c 0,0 0,9.673 0,9.673 0.888,0.02 1.776,0.041 2.653,-0.179 0.877,-0.219 1.745,-0.679 2.367,-1.541 0.622,-0.862 1,-2.127 0.98,-3.403 -0.02,-1.275 -0.439,-2.561 -1.193,-3.337 -0.755,-0.776 -1.847,-1.041 -2.704,-1.158 -0.857,-0.117 -1.48,-0.087 -2.102,-0.056
		m 11.908,12.245 v-14.785 h8.969 v2.51 h-5.786 v3.612 h5.388 v2.51 h-5.449 v6.092
		""";

	/// <summary>A download arrow, unioned with <see cref="PdfDocument"/> to mean "PDF not downloaded".</summary>
	public const string PdfDownArrow = """
		M29,44 V58.7498 H35.0491 A 1.5,1.5 0 0 1 36.1342,61.2861 L23.5607,73.8595 A 1.5,1.5 0 0 1 21.4393,73.8595 L8.8658,61.2861 A 1.5,1.5 0 0 1 9.9509,58.7498 H16 V44 A 1.5,1.5 0 0 1 17.5,42.5 H27.5 A 1.5,1.5 0 0 1 29,44
		""";

	/// <summary>A rounded square containing a minus bar. Natural size 64 x 64.</summary>
	public const string SeriesMinus = """
		M0,2 A 2,2 0 0 1 2,0 H62 A2,2 0 0 1 64,2 V62 A 2,2 0 0 1 62,64 H 2 A 2,2 0 0 1 0,62 V2
		M 2,2 H62 V62 H2 V2
		M11,28 h42 a 1,1 0 0 1 1,1 v6 a 1,1 0 0 1 -1,1 h-42 a 1,1 0 0 1 -1,-1 v-6 a 1,1 0 0 1 1,-1
		""";

	/// <summary>A vertical bar, unioned with <see cref="SeriesMinus"/> to turn the minus into a plus.</summary>
	public const string SeriesPlusBar
		= "M28,53 v-42 a 1,1 0 0 1 1,-1 h6 a 1,1 0 0 1 1,1 v42 a 1,1 0 0 1 -1,1 h-6 a 1,1 0 0 1 -1,-1";

	/// <summary>A "no entry" sign. Natural size 64 x 64.</summary>
	public const string BookError
		= "M32,0 a 32,32 0 0 1 0,64 a 32,32 0 0 1 0,-64 m 0,4 a 28,28 0 0 1 0,56 a 28,28 0 0 1 0,-56 m-21,24 h42 a 1,1 0 0 1 1,1 v6 a 1,1 0 0 1 -1,1 h-42 a 1,1 0 0 1 -1,-1 v-6 a 1,1 0 0 1 1,-1";

	#region Layout, in the same units as the artwork above

	/// <summary>The height the stoplight body is scaled to. Everything else is sized relative to it.</summary>
	public const float StoplightHeight = 64;

	/// <summary>Lamp size, in stoplight-body-scaled units.</summary>
	public const float LampWidth = 20;
	public const float LampHeight = 18;

	/// <summary>Lamp offsets from the top-left of the scaled stoplight body.</summary>
	public const float LampLeft = 5;
	public const float RedLampTop = 5;
	public const float YellowLampTop = 23;
	public const float GreenLampTop = 42;

	/// <summary>The width the PDF glyph is scaled to, and the gap between it and the stoplight.</summary>
	public const float PdfWidth = 28.8f;
	public const float PdfLeftMargin = 4;

	/// <summary>
	/// The Audible Plus badge's diameter, and how far right of the stoplight body's edge its center
	/// sits. The badge's top is flush with the icon's, so it costs no height and the stoplight is
	/// drawn at the same size whether or not a book is a Plus title.
	/// </summary>
	public const float PlusBadgeDiameter = 18;
	public const float PlusBadgeCornerOffset = 5;

	/// <summary>
	/// How wide a rim of nothing is cut out of the stoplight around the badge. Without it the
	/// circle runs straight into the body and the two read as one blobby shape.
	/// </summary>
	public const float PlusBadgeRimWidth = 2.5f;

	/// <summary>Keeps the badge's rim from reaching the PDF glyph on a Plus title which has a PDF.</summary>
	public const float PlusBadgePdfGap = 2;

	/// <summary>
	/// The badge's plus, as a fraction of the badge's diameter, and its bars' thickness as a
	/// fraction of that. The bars are far chunkier than the series plus's because the badge is a
	/// third of the icon's height, and a proportionally scaled plus would disappear in the grid.
	/// </summary>
	public const float PlusBadgeGlyphExtent = 0.55f;
	public const float PlusBadgeGlyphThickness = 0.3f;

	#endregion
}
