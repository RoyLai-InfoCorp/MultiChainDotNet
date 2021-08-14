using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Fluent.Base;
using MultiChainDotNet.Fluent.Signers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.MultiStageMultiSig
{
    public class MultiStageMultiSigBuilder: MultiSigBase, IAddMultiSigRawTransaction, IAddTransactionCommand, IAddSigner, IMultiSignPartial
	{
		ILogger<MultiChainFluentApi> _logger;
		string _raw;
		MultiChainTransactionCommand _txnCmd;
		private IList<SignerBase> _signers = new List<SignerBase>();

		public MultiStageMultiSigBuilder(ILogger<MultiChainFluentApi> logger)
		{
			_logger=logger;
		}

		public IAddTransactionCommand AddMultiSigRawTransaction(string multisigRawTxn)
		{
			_raw = multisigRawTxn;
			return this;
		}

		public IAddSigner AddTransactionCommand(MultiChainTransactionCommand txnCmd)
		{
			_txnCmd = txnCmd;
			return this;
		}

		public IMultiSignPartial AddSigner(SignerBase signer)
		{
			_signers.Add(signer);
			return this;
		}

		/// <summary>
		/// Return a list of signatures from signer.
		/// </summary>
		/// <param name="rawSendFrom"></param>
		/// <param name="redeemScript"></param>
		/// <returns></returns>
		public string[] MultiSignPartial(string redeemScript)
		{
			var txnHashes = CreateMultiSigTransactionHashes(_raw.Hex2Bytes(), redeemScript.Hex2Bytes());
			return MultiSignPartialAsync(_signers[0], txnHashes).Result.Select(x => x.Bytes2Hex()).ToArray();
		}

		protected List<byte[]> CreateMultiSigTransactionHashes(byte[] rawSendFrom, byte[] redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			var vin = MultiChainTxnHelper.GetVin(rawSendFrom);

			_logger.LogDebug($"Creating transaction for signing...");

			// Create temp script sig
			VarInt redeemScriptLen = new VarInt().Import(redeemScript);
			var tempScriptSig = redeemScriptLen.Bytes;
			tempScriptSig = tempScriptSig.Concat(redeemScript).ToArray();
			_logger.LogDebug($"tempScripSig:{tempScriptSig.Bytes2Hex()}");

			// Add temp script sig to each input
			List<byte[]> hashes = new List<byte[]>();
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				byte[] tempTxn = rawSendFrom;

				// Add temp script sig to each input
				_logger.LogDebug($"Update temp scriptsig for input {txinIndex}...");
				var scriptSigPos = MultiChainTxnHelper.GetScriptSigPosition(tempTxn, txinIndex);
				tempTxn = tempTxn.BlockReplace(scriptSigPos, BitCoinConstants.SCRIPTSIG_PLACEHOLDER_LENGTH, tempScriptSig);

				tempTxn = tempTxn.Concat(MultiChainTxnHelper.HashTypeCode((byte)hashType)).ToArray();
				_logger.LogTrace($"TempTransactionDecoded:{MultiChainTxnHelper.Decode(tempTxn)}");

				byte[] hash = tempTxn.SHA256().SHA256();

				hashes.Add(hash);
			}
			return hashes;
		}

	}
}
