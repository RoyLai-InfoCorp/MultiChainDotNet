// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using System;
using System.Linq;
using UtilsDotNet;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Core.Utils
{
	/// <summary>
	/// https://www.multichain.com/developers/address-key-format/
	/// </summary>
	/// <returns></returns>
	public static class MultiChainAddressHelper
	{

		public static byte[] InsertPubkeyVersion(string pubkeyHash, string version)
		{
			var pubkeyHashBytes = pubkeyHash.Hex2Bytes();
			var versionBytes = version.Hex2Bytes();

			var space = (int)Math.Floor(20f / versionBytes.Length);
			var extended = new byte[pubkeyHashBytes.Length + versionBytes.Length];
			for (int i = 0, j = 0; i < pubkeyHashBytes.Length; i += space, j++)
			{
				var len = space;
				if (i + space >= pubkeyHashBytes.Length)
					len = pubkeyHashBytes.Length - i;
				Buffer.BlockCopy(versionBytes, j, extended, i + j, 1);
				Buffer.BlockCopy(pubkeyHashBytes, i, extended, i + j + 1, len);
			}
			return extended;
		}

		public static byte[] InsertPrvkeyVersion(byte[] wifNoVersionNoChecksum, string prvkeyHashVersion)
		{
			// 3. Add the first version byte from the private-key-version blockchain parameter to the start of the private key. 
			// If it is more than one byte long, insert each subsequent byte of it after every floor(33/len(private-key-version)) 
			// bytes of the key => wifNoChecksum_bytes. 
			var version_bytes = prvkeyHashVersion.Hex2Bytes();
			var step = Math.Floor(Convert.ToDecimal(wifNoVersionNoChecksum.Length) / version_bytes.Length);
			var wifNoChecksum = new byte[wifNoVersionNoChecksum.Length + version_bytes.Length];
			for (int i = 0, k = 0; i < wifNoVersionNoChecksum.Length; i++)
			{
				if (i % step == 0 && k < version_bytes.Length)
				{
					Buffer.BlockCopy(version_bytes, k, wifNoChecksum, i + k, 1);
					k++;
				}
				Buffer.BlockCopy(wifNoVersionNoChecksum, i, wifNoChecksum, i + k, 1);
			}
			return wifNoChecksum;
		}

		public static byte[] RemovePubkeyVersion(string extendedPubkeyHash, string version)
		{
			var pubkeyHashBytes = extendedPubkeyHash.Hex2Bytes();
			var versionBytes = version.Hex2Bytes();
			var space = (int)Math.Floor(20f / versionBytes.Length);
			var newBytes = new byte[pubkeyHashBytes.Length - versionBytes.Length];
			for (int i = 1, j = 0; i < pubkeyHashBytes.Length; i++)
			{
				if (i % (space + 1) != 0)
				{
					newBytes[j] = pubkeyHashBytes[i];
					j++;
				}
			}
			return newBytes;
		}

		public static byte[] RemovePubkeyVersion(byte[] extendedPubkeyHash, string version)
		{
			return RemovePubkeyVersion(extendedPubkeyHash.Bytes2Hex(), version);
		}

		private static byte[] RemoveByteAt(byte[] source, int index)
		{
			byte[] final = new byte[source.Length - 1];

			if (index == 0)
			{
				Buffer.BlockCopy(source, 1, final, 0, source.Length - 1);
			}
			else
			{
				Buffer.BlockCopy(source, 0, final, 0, index);
				Buffer.BlockCopy(source, index + 1, final, index, source.Length - index - 1);
			}
			return final;
		}

		public static byte[] RemovePrivkeyVersion(byte[] wifNoChecksum, string version)
		{
			var version_bytes = version.Hex2Bytes();
			var version_bytes_len = version_bytes.Length;
			var wifNoChecksum_len = wifNoChecksum.Length;
			var step = Convert.ToInt32(Math.Floor(Convert.ToDecimal(33) / version_bytes.Length));

			var original = wifNoChecksum;
			// Remove first byte
			for (int counter = 0, index = 0; counter < 4; counter++)
			{
				if (index < original.Length)
				{
					byte[] reducedBytes = RemoveByteAt(original, index);
					original = reducedBytes;
				}
				index += step;
			}
			return original;
		}

		private static byte[] GetAddressFromPublicKey(byte[] compressedPubkey, string pubkeyVersion, string checksumValue)
		{
			byte[] pubkey = compressedPubkey;

			byte[] pubkeyhash = pubkey.SHA256().RIPEMD160();

			byte[] pubkeyhashVersioned = InsertPubkeyVersion(pubkeyhash.Bytes2Hex(), pubkeyVersion);

			byte[] checksum = ((byte[])pubkeyhashVersioned.Clone()).SHA256().SHA256().SafeSubarray(0, 4).XOR(checksumValue.Hex2Bytes());

			return pubkeyhashVersioned.Concat(checksum).ToArray();
		}

		public static string GetPrivateKeyFromWif(string wif, string prvkeyHashVersion, bool useCompressedKey = true)
		{
			//1. Convert from Base58 to hexadecimal wif
			byte[] wifHex = wif.Base582Bytes();

			//2. Remove last 4 bytes checksum
			byte[] wifNoChecksum = wifHex.SkipLast(4).ToArray();

			//3. Remove private key version
			var ptekey = RemovePrivkeyVersion(wifNoChecksum, prvkeyHashVersion);

			//4. If the public key of this private key is in compressed form, remove the last byte
			if (useCompressedKey)
				ptekey = RemoveByteAt(ptekey, ptekey.Length - 1);

			return ptekey.Bytes2Hex();
		}

		public static string GetPublicKeyFromPrivateKey(string privateKey, bool useCompressed = true)
		{
			return CryptoHelper.GenerateSecp256k1PublicKey(privateKey.Hex2Bytes(), useCompressed).Bytes2Hex();
		}

		public static string GetAddressFromPublicKey(string compressedPubkey, string pubkeyHashVersion, string checksumValue)
		{
			var addressHex = GetAddressFromPublicKey(compressedPubkey.Hex2Bytes(), pubkeyHashVersion, checksumValue);
			return addressHex.Bytes2Base58();
		}

		public static string GetWifFromPrivateKey(string pteKey, string prvkeyHashVersion, string checksumValue, bool useCompression = true)
		{
			// 1. Start with a raw private ECDSA key:
			byte[] wif = pteKey.Hex2Bytes();

			// 2. Add  0x01 at the end if this private key corresponds to a compressed public key => wifNoVersionNoChecksum_bytes
			if (useCompression)
				wif = wif.Concat(new byte[] { 0x01 }).ToArray();

			// 3. Merge private key version to the private key
			wif = InsertPrvkeyVersion(wif, prvkeyHashVersion);

			// 4. Calculate the checksum from the versioned private key
			byte[] checksum = ((byte[])wif.Clone())
						.SHA256()
						.SHA256()
						.SafeSubarray(0, 4)
						.XOR(checksumValue.Hex2Bytes());

			// 5. Add the checksum to the end of versioned private key
			wif = wif.Concat(checksum).ToArray();

			// 6. Return as Base58
			return wif.Bytes2Base58();
		}


		/// <summary>
		/// Convert the address to hexadecimal, strip away the version to get 24 bytes pk hash
		/// Convert the 24 bytes pk hash to Base64 = 32 bytes string.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="addressVersion"></param>
		/// <returns></returns>
		public static string Get32BytesNameFromAddress(string address, string pubkeyVersion)
		{
			byte[] bytes = address.Base582Bytes();
			var checkSum = bytes.SafeSubarray(bytes.Length - 5, 4);
			var addressByte24 = RemoveAddressVersion(bytes.SkipLast(4).ToArray(), pubkeyVersion.Hex2Bytes())
				.Concat(checkSum)
				.ToArray();
			return addressByte24.Bytes2Base64();
		}

		/// <summary>
		/// Convert the address to hexadecimal, strip away the version and checksum to get 20 bytes pk hash
		/// Convert the 20 bytes pk hash to Base64 = 28 bytes string.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="addressVersion"></param>
		/// <returns></returns>
		public static string Get28BytesNameFromAddress(string address, string pubkeyVersion)
		{
			byte[] bytes = address.Base582Bytes();
			var checkSum = bytes.SafeSubarray(bytes.Length - 5, 4);
			var addressByte20 = RemoveAddressVersion(bytes.SkipLast(4).ToArray(), pubkeyVersion.Hex2Bytes())
				.ToArray();
			return addressByte20.Bytes2Base64();
		}


		public static byte[] RemoveAddressVersion(byte[] pubkeyHashBytes, byte[] versionBytes)
		{
			var space = (int)Math.Floor(20f / versionBytes.Length);
			var newBytes = new byte[pubkeyHashBytes.Length - versionBytes.Length];
			for (int i = 1, j = 0; i < pubkeyHashBytes.Length; i++)
			{
				if (i % (space + 1) != 0)
				{
					newBytes[j] = pubkeyHashBytes[i];
					j++;
				}
			}
			return newBytes;
		}

		public static AddressData GenerateNewAddress(string pubkeyHashVersion, string ptekeyHashVersion, string checksumValue)
		{
			var data = new AddressData();
			data.Ptekey = CryptoHelper.GenerateSecp256k1PrivateKey().Bytes2Hex();
			data.Pubkey = CryptoHelper.GenerateSecp256k1PublicKey(data.Ptekey.Hex2Bytes()).Bytes2Hex();
			data.Address = GetAddressFromPublicKey(data.Pubkey,
				pubkeyHashVersion,
				checksumValue);
			data.Wif = GetWifFromPrivateKey(data.Ptekey,
				ptekeyHashVersion,
				checksumValue);
			return data;
		}



	}
}
