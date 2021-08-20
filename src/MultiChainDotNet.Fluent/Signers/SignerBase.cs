// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Utils;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Signers
{
	/// <summary>
	/// Reference:
	/// - https://en.bitcoin.it/w/images/en/7/70/Bitcoin_OpCheckSig_InDetail.png
	/// - https://en.bitcoin.it/wiki/OP_CHECKSIG
	/// - https://developer.bitcoin.org/devguide/transactions.html
	/// - http://www.righto.com/2014/02/bitcoins-hard-way-using-raw-bitcoin.html
	/// - https://bitcoin.stackexchange.com/questions/3374/how-to-redeem-a-basic-tx
	/// </summary>
	public abstract class SignerBase
	{
		public Guid Id { get; } = Guid.NewGuid();
		public abstract Task<string> GetPublicKeyAsync();

		protected abstract Task<byte[]> CreateSignatureBytesAsync(byte[] hash);

		/// <summary>
		/// The signature will be in DER Encoded form
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		public async Task<byte[]> SignAsync(byte[] hash)
		{
			byte[] signature = null;
			int sigLength = 0;

			while (sigLength != 0x47)
			{
				// Sign message
				byte[] sig = await CreateSignatureBytesAsync(hash);

				// Signature is returned in byte[] array form and needs to be converted to BigInt form to be DER encoded
				var ms = new MemoryStream(72);
				DerSequenceGenerator seq = new DerSequenceGenerator(ms);
				seq.AddObject(new DerInteger(new BigInteger(1, sig.Take(32).ToArray())));
				seq.AddObject(new DerInteger(new BigInteger(1, sig.Skip(32).ToArray())));
				seq.Close();

				//signature =  ms.ToArray().Bytes2Hex();
				//sigLength = (signature.Length / 2 + 1).ToString("x");

				signature = ms.ToArray();
				sigLength = signature.Length + 1;
			}
			return signature;
		}

		/// <summary>
		/// The signature will be in DER Encoded form
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>

		public async Task<string> SignAsync(string hash)
		{
			string signature = "";
			string sigLength = "";

			while (sigLength != "47")
			{
				// Sign message
				byte[] sig = await CreateSignatureBytesAsync(hash.Hex2Bytes());

				// Signature is returned in byte[] array form and needs to be converted to BigInt form to be DER encoded
				var ms = new MemoryStream(72);
				DerSequenceGenerator seq = new DerSequenceGenerator(ms);
				seq.AddObject(new DerInteger(new BigInteger(1, sig.Take(32).ToArray())));
				seq.AddObject(new DerInteger(new BigInteger(1, sig.Skip(32).ToArray())));
				seq.Close();

				signature = ms.ToArray().Bytes2Hex();
				sigLength = (signature.Length / 2 + 1).ToString("x");
			}
			return signature;
		}

	}
}
