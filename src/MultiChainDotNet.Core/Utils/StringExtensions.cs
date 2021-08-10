using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiChainDotNet.Core.Utils
{
	public static class StringExtensions
	{
		public static byte[] SHA256(byte[] data, int offset = 0, int count = 0)
		{
			var sha256 = new Sha256Digest();
			if (count == 0)
				count = data.Length;
			sha256.BlockUpdate(data, offset, count);
			var buffer = new byte[sha256.GetDigestSize()];
			sha256.DoFinal(buffer, 0);
			return buffer;
		}

		public static string SHA256(this string data)
		{
			var bytes = data.Hex2Bytes();
			var hashed = SHA256(bytes);
			return hashed.Bytes2Hex();
		}

		public static Guid ToGuid(this string target)
		{
			if (target is null)
				return Guid.Empty;
			return Guid.Parse(target);
		}

		public static string[] Repeat(this string data, int count)
		{
			if (string.IsNullOrEmpty(data))
				return null;

			string[] result = new string[count];
			for (int i = 0; i < count; i++)
				result[i] = data;
			return result;
		}

		/// <summary>
		/// Convert hexadecimal binary encoding to bytes
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static byte[] Hex2Bytes(this string data)
		{
			if (string.IsNullOrEmpty(data))
				return null;

			try
			{
				var s = data.ToLower();
				if (s.Length % 2 != 0)
					throw new Exception("Hexadecimal length should be even.");
				var bytes = Enumerable.Range(0, s.Length)
					.Where(x => x % 2 == 0)
					.Select(x => Convert.ToByte(s.Substring(x, 2), 16))
					.ToArray();
				return bytes;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e);
				return HexStringToBytes(data);
			}
		}

		/// <summary>
		/// Convert hexadecimal binary encoding to bytes
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		private static byte[] HexStringToBytes(string data)
		{
			if (string.IsNullOrEmpty(data))
				return null;

			var result = new List<byte>();

			for (var i = data.Length - 1; i >= 0; i -= 2)
			{
				result.Insert(0,
					i > 0
						? Convert.ToByte(data.Substring(i - 1, 2), 16)
						: Convert.ToByte(data.Substring(i, 1), 16));
			}

			return result.ToArray();
		}


		/// <summary>
		/// Convert hexadecimnal binary encoding to Base64 binary encoding
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static string Hex2Base64(this string data)
		{
			if (string.IsNullOrEmpty(data))
				return null;

			var input = Hex2Bytes(data.ToLower());
			return Convert.ToBase64String(input);
		}

		/// <summary>
		/// Convert hexadecimnal binary encoding to UTF8 text encoding
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static string Hex2UTF8(this string data)
		{
			if (string.IsNullOrEmpty(data))
				return null;

			var input = Hex2Bytes(data.ToLower());
			return Encoding.UTF8.GetString(input, 0, input.Length);
		}

		/// <summary>
		/// Convert Base64 binary encoding to hexadecimal binary encoding
		/// </summary>
		/// <param name="base64"></param>
		/// <returns></returns>
		public static byte[] Base642Bytes(this string data)
		{
			if (string.IsNullOrEmpty(data))
				return null;
			return Convert.FromBase64String(data);
		}

		/// <summary>
		/// Convert Base64 binary encoding to hexadecimal binary encoding
		/// </summary>
		/// <param name="base64"></param>
		/// <returns></returns>
		public static byte[] Base582Bytes(this string data)
		{
			if (string.IsNullOrEmpty(data))
				return null;
			return SimpleBase.Base58.Bitcoin.Decode(data).ToArray();

		}


		/// <summary>
		/// Convert Base64 binary encoding to hexadecimal binary encoding
		/// </summary>
		/// <param name="base64"></param>
		/// <returns></returns>
		public static string Base642Hex(this string base64)
		{
			var bytes = Convert.FromBase64String(base64);
			if (bytes != null)
				return BitConverter.ToString(bytes).ToLower().Replace("-", "");
			return null;
		}

		/// <summary>
		/// Convert UTF8 text encoding to hexadecimal binary encoding
		/// </summary>
		/// <param name="base64"></param>
		/// <returns></returns>
		public static string UTF82Hex(this string utf8)
		{
			var bytes = Encoding.UTF8.GetBytes(utf8);
			if (bytes != null)
				return BitConverter.ToString(bytes).ToLower().Replace("-", "");
			return null;
		}

		/// <summary>
		/// Convert UTF8 text encoding to hexadecimal binary encoding
		/// </summary>
		/// <param name="base64"></param>
		/// <returns></returns>
		public static byte[] UTF82Bytes(this string utf8)
		{
			var bytes = Encoding.UTF8.GetBytes(utf8);
			return bytes;
		}


	}
}
