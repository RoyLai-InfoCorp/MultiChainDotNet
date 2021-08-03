using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Text;

namespace MultiChainDotNet.Core.Utils
{
	public static class CryptoHelper
	{

		#region RSA

		public static AsymmetricCipherKeyPair GenerateRSAKeyPair()
		{
			var gen = new RsaKeyPairGenerator();
			gen.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
			return gen.GenerateKeyPair();
		}

		public static string GetRSAPublicKey(AsymmetricCipherKeyPair keyPair)
		{
			using (TextWriter t = new StringWriter())
			{
				var writer = new PemWriter(t);
				writer.WriteObject(keyPair.Public);
				return t.ToString();
			}
		}

		public static string GetRSAPrivateKey(AsymmetricCipherKeyPair keyPair)
		{
			using (TextWriter t = new StringWriter())
			{
				var writer = new PemWriter(t);
				writer.WriteObject(keyPair.Private);
				return t.ToString();
			}
		}

		/// <summary>
		/// Function is used for Encrypt from server.
		/// </summary>
		/// <param name="unencrypted"></param>
		/// <param name="pubKey"></param>
		/// <returns></returns>
		public static string RsaEncryptWithPublicKey(string unencrypted, string pubKey)
		{
			var bytesToEncrypt = Encoding.UTF8.GetBytes(unencrypted);
			var encryptEngine =
				new Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding(new Org.BouncyCastle.Crypto.Engines.RsaEngine());
			using (var txtreader = new StringReader(pubKey))
			{
				var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();
				encryptEngine.Init(true, keyParameter);
			}
			var encrypted =
				Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
			return encrypted;
		}

		/// <summary>
		/// Function is used for Encrypt from Mobile App.
		/// </summary>
		/// <param name="clearText"></param>
		/// <param name="privateKey"></param>
		/// <returns></returns>
		public static string RsaEncryptWithPrivateKey(string clearText, string privateKey)
		{
			var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

			var encryptEngine = new Pkcs1Encoding(new RsaEngine());

			using (var txtreader = new StringReader(privateKey))
			{
				var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();

				encryptEngine.Init(true, keyPair.Private);
			}

			var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
			return encrypted;
		}

		/// <summary>
		/// Function is used for Decrypt from server.
		/// </summary>
		/// <param name="base64Input"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static string RsaDecryptWithPublicKey(string base64Input, string publicKey)
		{
			var bytesToDecrypt = Convert.FromBase64String(base64Input);

			var decryptEngine = new Pkcs1Encoding(new RsaEngine());

			using (var txtreader = new StringReader(publicKey))
			{
				var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();

				decryptEngine.Init(false, keyParameter);
			}

			var decrypted = decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length).Bytes2UTF8();
			return decrypted;
		}

		/// <summary>
		/// Function is used for Decrypt from Mobile app.
		/// </summary>
		/// <param name="encrypted"></param>
		/// <param name="privKey"></param>
		/// <returns></returns>
		public static string RsaDecryptWithPrivateKey(string encrypted, string privKey)
		{
			var bytesToDecrypt = Convert.FromBase64String(encrypted);
			AsymmetricCipherKeyPair keyPair;
			var decryptEngine = new Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding(new RsaEngine());
			byte[] result;
			using (var txtreader = new StringReader(privKey))
			{
				keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();
				decryptEngine.Init(false, keyPair.Private);
				result = decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length);
			}
			var decrypted = Encoding.UTF8.GetString(result, 0, result.Length);
			return decrypted;
		}

		#endregion

		#region AES

		public static byte[] GenerateAESKey()
		{
			var random = new SecureRandom();
			var keyBytes = new byte[16];
			random.NextBytes(keyBytes);
			return keyBytes;
		}

		public static byte[] GenerateRandomIV(int size = 16)
		{
			var random = new SecureRandom();
			var iv = new byte[size];
			random.NextBytes(iv);
			return iv;
		}

		public static byte[] EncryptAES(byte[] inbytes, byte[] secret, byte[] iv)
		{
			var cipher =
				new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); //Default scheme is PKCS5/PKCS7
			var keyParam =
				new ParametersWithIV(new KeyParameter(secret), iv, 0, iv.Length);

			cipher.Init(true, keyParam);
			var outbytes = new byte[cipher.GetOutputSize(inbytes.Length)];
			var length = cipher.ProcessBytes(inbytes, outbytes, 0);
			cipher.DoFinal(outbytes, length);
			return outbytes;
		}

		public static byte[] EncryptAES(byte[] inbytes, byte[] secret)
		{
			var cipher =
				new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); //Default scheme is PKCS5/PKCS7
			var keyParam =
				new ParametersWithIV(new KeyParameter(secret), new byte[16], 0, 16);
			cipher.Init(true, keyParam);
			var outbytes = new byte[cipher.GetOutputSize(inbytes.Length)];
			var length = cipher.ProcessBytes(inbytes, outbytes, 0);
			cipher.DoFinal(outbytes, length);
			return outbytes;
		}

		public static byte[] DecryptAES(byte[] inbytes, byte[] secret, byte[] iv = null)
		{
			if (iv is null)
				iv = new byte[16];
			var cipher =
				new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); //Default scheme is PKCS5/PKCS7
			var keyParam =
				new ParametersWithIV(new KeyParameter(secret), iv, 0, iv.Length);
			cipher.Init(false, keyParam);
			var decryption_buffer = new byte[cipher.GetOutputSize(inbytes.Length)];
			var initial_length = cipher.ProcessBytes(inbytes, decryption_buffer, 0);
			var last_bytes = cipher.DoFinal(decryption_buffer, initial_length);

			// Strip away the '/0' before returning
			var total_bytes = initial_length + last_bytes;
			var result = new byte[total_bytes];
			Array.Copy(decryption_buffer, result, total_bytes);
			return result;
		}

		#endregion

		#region Hash

		public static byte[] RIPEMD160(byte[] data)
		{
			var ripemd160 = new RipeMD160Digest();
			ripemd160.BlockUpdate(data, 0, data.Length);
			var buffer = new byte[ripemd160.GetDigestSize()];
			ripemd160.DoFinal(buffer, 0);
			return buffer;
		}

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
		#endregion

		#region ECDSA

		public static byte[] GenerateSecp256k1PrivateKey()
		{
			var ecc = SecNamedCurves.GetByName("secp256k1");
			var DomainParams = new ECDomainParameters(ecc.Curve, ecc.G, ecc.N, ecc.H);

			// Generate EC Key Pair
			//add algorithm type "ECDSA" in ECKeyPairGenerator()
			var keyGen = new ECKeyPairGenerator("ECDSA");
			var random = new SecureRandom();
			var keyParams = new ECKeyGenerationParameters(DomainParams, random);
			keyGen.Init(keyParams);
			var keyPair = keyGen.GenerateKeyPair();
			var privateKeyParams = (ECPrivateKeyParameters)keyPair.Private;

			// Get Private Key
			var privD = privateKeyParams.D;
			var privBytes = privD.ToByteArray();
			var check = false;
			var privKey = privBytes.Bytes2Hex();
			int i;
			if (!int.TryParse(privKey.Substring(0, 1), out i))
			{
				check = false;
			}
			while (privBytes.Length != 32 || !check)
			{
				keyPair = keyGen.GenerateKeyPair();
				privateKeyParams = (ECPrivateKeyParameters)keyPair.Private;
				privD = privateKeyParams.D;
				privBytes = privD.ToByteArray();
				privKey = privBytes.Bytes2Hex();
				if (!int.TryParse(privKey.Substring(0, 1), out i))
				{
					check = false;
				}
				check = true;
			}
			return privBytes;
		}

		public static byte[] GenerateSecp256k1PublicKey(byte[] privateKey, bool useCompression = true, bool useNormalize = true)
		{
			var ecc = SecNamedCurves.GetByName("secp256k1");
			var domainParams = new ECDomainParameters(ecc.Curve, ecc.G, ecc.N, ecc.H);
			var d = new BigInteger(privateKey);
			var q = domainParams.G.Multiply(d);

			var q1 = q.GetEncoded();
			var q2 = q.GetEncoded(false);

			//if (useNormalize)
				//q = q.Normalize();
			return q.GetEncoded(useCompression);
		}

		#endregion

		public static string HashPassword(string Password, string Salt)
		{
			var PasswordSalt = Password + Salt;
			return SHA256(PasswordSalt.UTF82Bytes()).Bytes2Hex();
		}

	}
}
