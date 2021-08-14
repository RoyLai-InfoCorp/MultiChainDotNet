using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Fluent.Base;

namespace MultiChainDotNet.Fluent
{
	public sealed class MultiSigSender
	{
		private readonly ILogger _logger;
		private IList<SignerBase> _signers = new List<SignerBase>();
		protected MultiChainTransactionCommand _txnCmd;
		private byte[] _rawSendFrom;
		private string _signed;

		public MultiSigSender(ILogger<MultiSigSender> logger, MultiChainTransactionCommand txnCmd)
		{
			_logger = logger;
			_txnCmd = txnCmd;
		}

		public MultiSigSender AddSigner(SignerBase signer)
		{
			_signers.Add(signer);
			return this;
		}

		public string RawSigned()
		{
			return _signed;
		}

		public string Send()
		{
			return Task.Run(async () =>
			{
				var result = await _txnCmd.SendRawTransactionAsync(_signed);
				if (result.IsError)
					throw result.Exception;
				return result.Result;
			}).GetAwaiter().GetResult();
		}


		public MultiSigSender MultiSign(string raw, string redeemScript)
		{
			_rawSendFrom = raw.Hex2Bytes();
			_logger.LogDebug($"InitTransaction:{_rawSendFrom.Bytes2Hex()}");
			_txnCmd.DecodeRawTransactionAsync(_rawSendFrom.Bytes2Hex()).Wait();
			_signed = MultiSignAsync(_signers, _rawSendFrom, redeemScript.Hex2Bytes()).Result;
			return this;
		}

		public async Task<string> MultiSignAsync(string raw, string redeemScript)
		{
			return await MultiSignAsync(_signers, _rawSendFrom, redeemScript.Hex2Bytes());
		}

		public MultiSigSender MultiSign(string raw, string redeemScript, IList<string[]> signersSignatures)
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
			_signed = signed.Bytes2Hex();

			return this;
		}


		/// <summary>
		/// Return a list of signatures from signer.
		/// </summary>
		/// <param name="rawSendFrom"></param>
		/// <param name="redeemScript"></param>
		/// <returns></returns>
		public string[] MultiSignPartial(string raw, string redeemScript)
		{
			var txnHashes = CreateMultiSigTransactionHashes(raw.Hex2Bytes(), redeemScript.Hex2Bytes());
			return MultiSignPartialAsync(_signers[0], txnHashes).Result.Select(x => x.Bytes2Hex()).ToArray();
		}


		/// <summary>
		/// For multisignation signing it has to be done in 2 parts:
		/// Part 1- Create individual signer's script sig FOR EACH INPUT.
		/// 1. Create the temporary transaction from createrawsendfrom and leave all input scriptsig empty.
		/// 2. Start with first input, replace it with the same redeemscript then add hash type code at the end and hash it.
		/// 3. Repeat (2) for the rest of the inputs.
		/// 4. This should result in a transaction hash for each input.
		/// 5. This can be done first without involving signers.
		/// 
		/// Part 2- Create the scriptsig and transaction
		/// 1. This part can only proceed once the required number of signers are available at the same time.
		/// This is because the scriptsig needs the signature to be appended
		/// in the same order as the way the multisig address is constructed.
		/// 2. Get the earlier temporary transaction created using createrawsendfrom.
		/// 3. For each input of the empty transaction, replace it with the final scriptsig.
		/// </summary>
		/// <param name="signers"></param>
		/// <param name="rawSendFrom"></param>
		/// <param name="redeemScript"></param>
		/// <param name="hashType"></param>
		/// <returns></returns>
		public async Task<string> MultiSignAsync(IList<SignerBase> signers, byte[] rawSendFrom, byte[] redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			var vin = MultiChainTxnHelper.GetVin(rawSendFrom);

			// STEP 1: Create the list of temporary transaction hashes, one for each transaction input.
			_logger.LogDebug($"Creating transaction for signing...");
			var txnHashes = CreateMultiSigTransactionHashes(rawSendFrom, redeemScript);

			// STEP 2: Pass the list of transaction hashes to each signer to sign.
			// This results in a dictionary of signers with list of signatures corresponding to each input.
			// Do not combine STEP 2 and 3!!
			// This is because the multisig transaction expects the signers to sign in certain order following the multisig address creation.
			// STEP 2 is used to collect the signatures first then use step 3 to re-arrange the signatures in the right order.
			Dictionary<Guid, List<byte[]>> signerSignatures = new Dictionary<Guid, List<byte[]>>();
			foreach (var signer in signers)
			{
				var signatures = await MultiSignPartialAsync(signer, txnHashes);
				signerSignatures[signer.Id] = signatures;
			}

			// STEP 3 Create the transaction using the signatures collected and the redeem script
			var finalTxn = rawSendFrom;
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				byte[] scriptSig = new byte[] { 0x00 };
				foreach (var signer in signers)
				{
					var signature = signerSignatures[signer.Id][txinIndex];
					scriptSig = scriptSig.Concat(signature).ToArray();
				}
				// Append the redeemscript
				byte[] redeemScriptLen = MultiChainTxnHelper.CreatePushDataOpCode((uint)redeemScript.Length);
				scriptSig = scriptSig.Concat(redeemScriptLen).ToArray();
				scriptSig = scriptSig.Concat(redeemScript).ToArray();

				// Encode the length of the new sigscript
				var scriptSigLen = new VarInt().Import(scriptSig);
				scriptSig = scriptSigLen.Bytes.Concat(scriptSig).ToArray();

				var scriptSigPos = MultiChainTxnHelper.GetScriptSigPosition(finalTxn, txinIndex);
				finalTxn = finalTxn.BlockReplace(scriptSigPos, BitCoinConstants.SCRIPTSIG_PLACEHOLDER_LENGTH, scriptSig);
			}

			_logger.LogDebug($"Final Transaction: {finalTxn.Bytes2Hex()}");
			_logger.LogDebug($"Final Transaction Decoded: {MultiChainTxnHelper.Decode(finalTxn)}");
			return finalTxn.Bytes2Hex();
		}

		private async Task<List<byte[]>> MultiSignPartialAsync(SignerBase signer, List<byte[]> txnHashes, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
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

		// List of transaction hashes from temp transaction equal to the number of inputs.
		private List<byte[]> CreateMultiSigTransactionHashes(byte[] rawSendFrom, byte[] redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
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

		public string CreateMultiSigTransactionHashes(string raw, string redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			return JsonConvert.SerializeObject(CreateMultiSigTransactionHashes(raw.Hex2Bytes(), redeemScript.Hex2Bytes(),hashType));
		}

	}
}
