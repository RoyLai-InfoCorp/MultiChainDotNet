using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Fluent.Base;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2
{
    public class MultiSigBase
    {
		protected async Task<List<byte[]>> MultiSignPartialAsync(SignerBase signer, List<byte[]> txnHashes, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			var signatures = new List<byte[]>();
			foreach (var txnHash in txnHashes)
			{
				// Create the signature using the txn hash and append the hashtype.
				var signature = await signer.SignAsync(txnHash);
				signature = signature.Append((byte)hashType).ToArray();
				VarInt signatureLen = new VarInt().Import(signature);
				signature = signatureLen.Bytes.Concat(signature).ToArray();
				signatures.Add(signature);
			}
			return signatures;
		}

		protected string MultiSign(string raw, string redeemScript, IList<string[]> signersSignatures)
		{
			var signed = raw.Hex2Bytes();
			var vin = MultiChainTxnHelper.GetVin(signed);
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				byte[] scriptSig = new byte[] { 0x00 };
				foreach (var signatureList in signersSignatures)
				{
					var signature = signatureList[txinIndex].Hex2Bytes();
					scriptSig = scriptSig.Concat(signature).ToArray();
				}
				// Append the redeemscript
				byte[] redeemScriptLen = MultiChainTxnHelper.CreatePushDataOpCode((uint)redeemScript.Hex2Bytes().Length);
				scriptSig = scriptSig.Concat(redeemScriptLen).ToArray();
				scriptSig = scriptSig.Concat(redeemScript.Hex2Bytes()).ToArray();

				// Encode the length of the new sigscript
				var scriptSigLen = new VarInt().Import(scriptSig);
				scriptSig = scriptSigLen.Bytes.Concat(scriptSig).ToArray();

				var scriptSigPos = MultiChainTxnHelper.GetScriptSigPosition(signed, txinIndex);
				signed = signed.BlockReplace(scriptSigPos, BitCoinConstants.SCRIPTSIG_PLACEHOLDER_LENGTH, scriptSig);
			}
			return signed.Bytes2Hex();
		}

	}
}
