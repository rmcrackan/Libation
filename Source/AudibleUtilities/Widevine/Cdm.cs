using Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AudibleUtilities.Widevine;

public enum KeyType
{
	/// <summary>
	/// Exactly one key of this type must appear.
	/// </summary>
	Signing = 1,
	/// <summary>
	/// Content key.
	/// </summary>
	Content = 2,
	/// <summary>
	/// Key control block for license renewals. No key.
	/// </summary>
	KeyControl = 3,
	/// <summary>
	/// wrapped keys for auxiliary crypto operations.
	/// </summary>
	OperatorSession = 4,
	/// <summary>
	/// Entitlement keys.
	/// </summary>
	Entitlement = 5,
	/// <summary>
	/// Partner-specific content key.
	/// </summary>
	OemContent = 6,
}

public interface ISession : IDisposable
{
	string GetLicenseChallenge(MpegDash dash);
	WidevineKey[] ParseLicense(string licenseMessage);
}

public class WidevineKey
{
	public Guid Kid { get; }
	public KeyType Type { get; }
	public byte[] Key { get; }
	internal WidevineKey(Guid kid, License.Types.KeyContainer.Types.KeyType type, byte[] key)
	{
		Kid = kid;
		Type = (KeyType)type;
		Key = key;
	}
	public override string ToString() => $"{Convert.ToHexString(Kid.ToByteArray(bigEndian: true)).ToLower()}:{Convert.ToHexString(Key).ToLower()}";
}

public partial class Cdm
{
	public static Guid WidevineContentProtection { get; } = new("edef8ba9-79d6-4ace-a3c8-27dcd51d21ed");
	private const int MAX_NUM_OF_SESSIONS = 16;
	internal Device Device { get; }

	private ConcurrentDictionary<Guid, Session> Sessions { get; } = new(-1, MAX_NUM_OF_SESSIONS);

	internal Cdm(Device device)
	{
		Device = device;
	}

	public ISession OpenSession()
	{
		if (Sessions.Count == MAX_NUM_OF_SESSIONS)
			throw new Exception("Too Many Sessions");

		var session = new Session(Sessions.Count + 1, this);

		var ddd = Sessions.TryAdd(session.Id, session);
		return session;
	}

	#region Session

	internal class Session : ISession
	{
		public Guid Id { get; } = Guid.NewGuid();
		private int SessionNumber { get; }
		private Cdm Cdm { get; }
		private byte[]? EncryptionContext { get; set; }
		private byte[]? AuthenticationContext { get; set; }

		public Session(int number, Cdm cdm)
		{
			SessionNumber = number;
			Cdm = cdm;
		}

		private string GetRequestId()
			=> $"{RandomUint():x8}00000000{Convert.ToHexString(BitConverter.GetBytes((long)SessionNumber)).ToLowerInvariant()}";

		public void Dispose()
		{
			if (Cdm.Sessions.ContainsKey(Id))
				Cdm.Sessions.TryRemove(Id, out var session);
		}

		public string GetLicenseChallenge(MpegDash dash)
		{
			if (!dash.TryGetPssh(Cdm.WidevineContentProtection, out var pssh))
				throw new InvalidDataException("No Widevine PSSH found in DASH");

			var licRequest = new LicenseRequest
			{
				ClientId = Cdm.Device.ClientId,
				ContentId = new()
				{
					WidevinePsshData = new()
					{
						LicenseType = LicenseType.Offline,
						RequestId = ByteString.CopyFrom(GetRequestId(), Encoding.ASCII)
					}
				},
				Type = LicenseRequest.Types.RequestType.New,
				RequestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				ProtocolVersion = ProtocolVersion.Version21,
				KeyControlNonce = RandomUint()
			};

			licRequest.ContentId.WidevinePsshData.PsshData.Add(ByteString.CopyFrom(pssh.InitData));

			var licRequestBts = licRequest.ToByteArray();
			EncryptionContext = CreateContext("ENCRYPTION", 128, licRequestBts);
			AuthenticationContext = CreateContext("AUTHENTICATION", 512, licRequestBts);

			var signedMessage = new SignedMessage
			{
				Type = SignedMessage.Types.MessageType.LicenseRequest,
				Msg = ByteString.CopyFrom(licRequestBts),
				Signature = ByteString.CopyFrom(Cdm.Device.SignMessage(licRequestBts))
			};

			return Convert.ToBase64String(signedMessage.ToByteArray());
		}

		public WidevineKey[] ParseLicense(string licenseMessage)
		{
			if (EncryptionContext is null || AuthenticationContext is null)
				throw new InvalidOperationException($"{nameof(GetLicenseChallenge)}() must be called before calling {nameof(ParseLicense)}()");

			var signedMessage = SignedMessage.Parser.ParseFrom(Convert.FromBase64String(licenseMessage));
			if (signedMessage.Type != SignedMessage.Types.MessageType.License)
				throw new InvalidDataException("Invalid license");

			var sessionKey = Cdm.Device.DecryptSessionKey(signedMessage.SessionKey.ToByteArray());

			if (!VerifySignature(signedMessage, AuthenticationContext, sessionKey))
				throw new InvalidDataException("Message signature is invalid");

			var license = License.Parser.ParseFrom(signedMessage.Msg);
			var keyToTheKeys = DeriveKey(sessionKey, EncryptionContext, 1);

			return DecryptKeys(keyToTheKeys, license.Key);
		}

		private static WidevineKey[] DecryptKeys(byte[] keyToTheKeys, IList<License.Types.KeyContainer> licenseKeys)
		{
			using var aes = Aes.Create();
			aes.Key = keyToTheKeys;
			var keys = new WidevineKey[licenseKeys.Count];

			for (int i = 0; i < licenseKeys.Count; i++)
			{
				var keyContainer = licenseKeys[i];

				var keyBytes = aes.DecryptCbc(keyContainer.Key.ToByteArray(), keyContainer.Iv.ToByteArray(), PaddingMode.PKCS7);
				var id = keyContainer.Id.ToByteArray();

				if (id.Length > 16)
				{
					var tryB64 = new byte[id.Length * 3 / 4];
					if (Convert.TryFromBase64String(Encoding.ASCII.GetString(id), tryB64, out int bytesWritten))
					{
						id = tryB64;
					}
					Array.Resize(ref id, 16);
				}
				else if (id.Length < 16)
				{
					id = id.Append(new byte[16 - id.Length]);
				}

				keys[i] = new WidevineKey(new Guid(id, bigEndian: true), keyContainer.Type, keyBytes);
			}
			return keys;
		}

		private static bool VerifySignature(SignedMessage signedMessage, byte[] authContext, byte[] sessionKey)
		{
			var mac_key_server = DeriveKey(sessionKey, authContext, 1).Append(DeriveKey(sessionKey, authContext, 2));

			var hmacData = (signedMessage.OemcryptoCoreMessage?.ToByteArray() ?? []).Append(signedMessage.Msg?.ToByteArray() ?? []);

			var computed_signature = HMACSHA256.HashData(mac_key_server, hmacData);

			return computed_signature.SequenceEqual(signedMessage.Signature);
		}

		private static byte[] DeriveKey(byte[] session_key, byte[] context, int counter)
		{
			var data = new byte[context.Length + 1];
			Array.Copy(context, 0, data, 1, context.Length);
			data[0] = (byte)counter;

			return AESCMAC(session_key, data);
		}

		private static byte[] AESCMAC(byte[] key, byte[] data)
		{
			using var aes = Aes.Create();
			aes.Key = key;

			// SubKey generation
			// step 1, AES-128 with key K is applied to an all-zero input block.
			byte[] subKey = aes.EncryptCbc(new byte[16], new byte[16], PaddingMode.None);

			nextSubKey();

			// MAC computing
			if ((data.Length == 0) || (data.Length % 16 != 0))
			{
				// If the size of the input message block is not equal to a positive
				// multiple of the block size (namely, 128 bits), the last block shall
				// be padded with 10^i
				nextSubKey();
				var padLen = 16 - data.Length % 16;
				Array.Resize(ref data, data.Length + padLen);
				data[^padLen] = 0x80;
			}

			// the last block shall be exclusive-OR'ed with K1 before processing
			for (int j = 0; j < subKey.Length; j++)
				data[data.Length - 16 + j] ^= subKey[j];

			// The result of the previous process will be the input of the last encryption.
			byte[] encResult = aes.EncryptCbc(data, new byte[16], PaddingMode.None);

			byte[] HashValue = new byte[16];
			Array.Copy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);

			return HashValue;

			void nextSubKey()
			{
				const byte const_Rb = 0x87;
				if (Rol(subKey) != 0)
					subKey[15] ^= const_Rb;

				static int Rol(byte[] b)
				{
					int carry = 0;

					for (int i = b.Length - 1; i >= 0; i--)
					{
						ushort u = (ushort)(b[i] << 1);
						b[i] = (byte)((u & 0xff) + carry);
						carry = (u & 0xff00) >> 8;
					}
					return carry;
				}
			}
		}

		private static byte[] CreateContext(string label, int keySize, byte[] licRequestBts)
		{
			var contextSize = label.Length + 1 + licRequestBts.Length + sizeof(int);

			var context = new byte[contextSize];
			var numChars = Encoding.ASCII.GetBytes(label.AsSpan(), context);
			Array.Copy(licRequestBts, 0, context, numChars + 1, licRequestBts.Length);

			var numBts = BitConverter.GetBytes(keySize);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(numBts);
			Array.Copy(numBts, 0, context, context.Length - sizeof(int), sizeof(int));
			return context;
		}

		private static uint RandomUint()
		{
			var bts = new byte[4];
			new Random().NextBytes(bts);
			return BitConverter.ToUInt32(bts, 0);
		}
	}

	#endregion
}
