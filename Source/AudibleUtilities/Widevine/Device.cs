using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace AudibleUtilities.Widevine;

internal enum DeviceTypes : byte
{
	Unknown = 0,
	Chrome = 1,
	Android = 2
}

internal class Device
{
	public DeviceTypes Type { get; }
	public int FileVersion { get; }
	public int SecurityLevel { get; }
	public int Flags { get; }

	public RSA CdmKey { get; }
	internal ClientIdentification ClientId { get; }

	public Device(Span<byte> fileData)
	{
		if (fileData.Length < 7 || fileData[0] != 'W' || fileData[1] != 'V' || fileData[2] != 'D')
			throw new InvalidDataException();

		FileVersion = fileData[3];
		Type = (DeviceTypes)fileData[4];
		SecurityLevel = fileData[5];
		Flags = fileData[6];

		if (FileVersion != 2)
			throw new InvalidDataException($"Unknown CDM File Version: '{FileVersion}'");

		var privateKeyLength = (fileData[7] << 8) | fileData[8];

		if (privateKeyLength <= 0 || fileData.Length < 9 + privateKeyLength + 2)
			throw new InvalidDataException($"Invalid private key length: '{privateKeyLength}'");

		var clientIdLength = (fileData[9 + privateKeyLength] << 8) | fileData[10 + privateKeyLength];

		if (clientIdLength <= 0 || fileData.Length < 11 + privateKeyLength + clientIdLength)
			throw new InvalidDataException($"Invalid client id length: '{clientIdLength}'");

		ClientId = ClientIdentification.Parser.ParseFrom(fileData.Slice(11 + privateKeyLength));
		CdmKey = RSA.Create();
		CdmKey.ImportRSAPrivateKey(fileData.Slice(9, privateKeyLength), out _);
	}

	public byte[] SignMessage(byte[] message)
	{
		var digestion = SHA1.HashData(message);
		return PssSha1Signer.SignHash(CdmKey, digestion);
	}

	public bool VerifyMessage(byte[] message, byte[] signature)
	{
		var digestion = SHA1.HashData(message);
		return CdmKey.VerifyHash(digestion, signature, HashAlgorithmName.SHA1, RSASignaturePadding.Pss);
	}

	public byte[] DecryptSessionKey(byte[] sessionKey)
		=> CdmKey.Decrypt(sessionKey, RSAEncryptionPadding.OaepSHA1);

    /// <summary>
    /// Completely managed implementation of RSASSA-PSS using SHA-1.
    /// https://github.com/bcgit/bc-csharp/blob/master/crypto/src/crypto/signers/PssSigner.cs
    /// 
    /// Absolutely nobody anywhere should use this RSASSA-PSS implementation in anything where they care about security at all. We completely skipped the random salt part of it because libation doesn't need security; it only needs to satisfy Audible server's challenge-response requirements.
	/// </summary>
	private static class PssSha1Signer
	{
		private const int Sha1DigestSize = 20;
		private const int Trailer = 0xBC;

		public static byte[] SignHash(RSA rsa, ReadOnlySpan<byte> hash)
		{
			ArgumentOutOfRangeException.ThrowIfNotEqual(hash.Length, Sha1DigestSize);

			var parameters = rsa.ExportParameters(true);
			var Modulus = new BigInteger(parameters.Modulus, isUnsigned: true, isBigEndian: true);
			var Exponent = new BigInteger(parameters.D, isUnsigned: true, isBigEndian: true);
			var emBits = rsa.KeySize - 1;
			var block = new byte[(emBits + 7) / 8];
			var firstByteMask = (byte)(0xFFU >> ((block.Length * 8) - emBits));

			Span<byte> mDash = new byte[8 + 2 * Sha1DigestSize];

			hash.CopyTo(mDash.Slice(8));
			var h = SHA1.HashData(mDash);

			block[^(2 * (Sha1DigestSize + 1))] = 1;
			byte[] dbMask = MaskGeneratorFunction1(h, 0, h.Length, block.Length - Sha1DigestSize - 1);
			for (int i = 0; i != dbMask.Length; i++)
				block[i] ^= dbMask[i];

			h.CopyTo(block, block.Length - Sha1DigestSize - 1);

			block[0] &= firstByteMask;
			block[^1] = Trailer;

			var input = new BigInteger(block, isUnsigned: true, isBigEndian: true);
			var result = BigInteger.ModPow(input, Exponent, Modulus);
			return result.ToByteArray(isUnsigned: true, isBigEndian: true);
		}

		private static byte[] MaskGeneratorFunction1(byte[] Z, int zOff, int zLen, int length)
		{
			byte[] mask = new byte[length];
			byte[] hashBuf = new byte[Sha1DigestSize];
			byte[] C = new byte[4];
			int counter = 0;

			using var sha = SHA1.Create();

			for (; counter < (length / Sha1DigestSize); counter++)
			{
				ItoOSP(counter, C);

				sha.TransformBlock(Z, zOff, zLen, null, 0);
				sha.TransformFinalBlock(C, 0, C.Length);

				sha.Hash!.CopyTo(mask, counter * Sha1DigestSize);
			}

			if ((counter * Sha1DigestSize) < length)
			{
				ItoOSP(counter, C);

				sha.TransformBlock(Z, zOff, zLen, null, 0);
				sha.TransformFinalBlock(C, 0, C.Length);

				Array.Copy(sha.Hash!, 0, mask, counter * Sha1DigestSize, mask.Length - (counter * Sha1DigestSize));
			}

			return mask;
		}

		private static void ItoOSP(int i, byte[] sp)
		{
			sp[0] = (byte)((uint)i >> 24);
			sp[1] = (byte)((uint)i >> 16);
			sp[2] = (byte)((uint)i >> 8);
			sp[3] = (byte)((uint)i >> 0);
		}
	}
}
