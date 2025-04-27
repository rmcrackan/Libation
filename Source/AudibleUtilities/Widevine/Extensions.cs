using System;

#nullable enable
namespace AudibleUtilities.Widevine;

internal static class Extensions
{
	public static T[] Append<T>(this T[] message, T[] appendData)
	{
		var origLength = message.Length;
		Array.Resize(ref message, origLength + appendData.Length);
		Array.Copy(appendData, 0, message, origLength, appendData.Length);
		return message;
	}
}
