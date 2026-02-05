using Mpeg4Lib.Boxes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AudibleUtilities.Widevine;

public class MpegDash
{
	private const string MpegDashNamespace = "urn:mpeg:dash:schema:mpd:2011";
	private const string CencNamespace = "urn:mpeg:cenc:2013";
	private const string UuidPreamble = "urn:uuid:";
	private XElement DashMpd { get; }
	private static XmlNamespaceManager NamespaceManager { get; } = new(new NameTable());
	static MpegDash()
	{
		NamespaceManager.AddNamespace("dash", MpegDashNamespace);
		NamespaceManager.AddNamespace("cenc", CencNamespace);
	}

	public MpegDash(Stream contents)
	{
		DashMpd = XElement.Load(contents);
	}

	public bool TryGetUri(Uri baseUri, [NotNullWhen(true)] out Uri? fileUri)
	{
		foreach (var baseUrl in DashMpd.XPathSelectElements("/dash:Period/dash:AdaptationSet/dash:Representation/dash:BaseURL", NamespaceManager))
		{
			try
			{
				fileUri = new Uri(baseUri, baseUrl.Value);
				return true;
			}
			catch
			{
				fileUri = null;
				return false;
			}
		}
		fileUri = null;
		return false;
	}

	public bool TryGetPssh(Guid protectionSystemId, [NotNullWhen(true)] out PsshBox? pssh)
	{
		foreach (var psshEle in DashMpd.XPathSelectElements("/dash:Period/dash:AdaptationSet/dash:ContentProtection/cenc:pssh", NamespaceManager))
		{
			if (psshEle?.Value?.Trim() is string psshStr
				&& psshEle.Parent?.Attribute(XName.Get("schemeIdUri")) is XAttribute scheme
				&& scheme.Value is string uuid
				&& uuid.Equals(UuidPreamble + protectionSystemId.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				Span<byte> buffer = new byte[psshStr.Length * 3 / 4];
				if (Convert.TryFromBase64String(psshStr, buffer, out var written))
				{
					using var ms = new MemoryStream(buffer.Slice(0, written).ToArray());
					pssh = BoxFactory.CreateBox(ms, null) as PsshBox;
					return pssh is not null;
				}
			}
		}
		pssh = null;
		return false;
	}
}
