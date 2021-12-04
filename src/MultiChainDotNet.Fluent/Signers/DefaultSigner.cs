// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Utils;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Fluent.Signers
{
    public sealed class DefaultSigner : SignerBase
    {
		string _privKey = null;
		public DefaultSigner(string privKey)
		{
			_privKey = privKey;
		}
		public DefaultSigner(byte[] privKey)
		{
			_privKey = privKey.Bytes2Hex();
		}


		public override Task<string> GetPublicKeyAsync()
		{
			var bytes = _privKey.Hex2Bytes();
			var pubkey = CryptoHelper.GenerateSecp256k1PublicKey(bytes, true);
			return Task.FromResult(pubkey.Bytes2Hex());
		}

		protected override Task<byte[]> CreateSignatureBytesAsync(byte[] bytes)
		{
			if (_privKey == null)
				throw new Exception("Cannot create signature because private key is null.");
			X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
			ECDomainParameters dom = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
			ECKeyParameters paramsx = new ECPrivateKeyParameters
				(new BigInteger(1, _privKey.Hex2Bytes()), dom);
			ECDsaSigner signer = new ECDsaSigner();
			signer.Init(true, paramsx);
			BigInteger[] sig = signer.GenerateSignature(bytes);

			var bytes1 = sig[0].ToByteArray();
			var bytes2 = sig[1].ToByteArray();
			var buffer = new byte[bytes1.Length + bytes2.Length];
			System.Buffer.BlockCopy(bytes1, 0, buffer, 0, bytes1.Length);
			System.Buffer.BlockCopy(bytes2, 0, buffer, bytes1.Length, bytes2.Length);
			return Task.FromResult(buffer);
		}

		public Task<bool> VerifySignatureAsync(string digest, string signature)
		{
			throw new NotImplementedException();
		}

	}
}
