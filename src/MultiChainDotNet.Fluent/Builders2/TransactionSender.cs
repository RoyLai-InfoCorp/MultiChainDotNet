using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using System.Linq;
using System.Text;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Core.Utils;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Fluent.Base;
using MultiChainDotNet.Core.MultiChainTransaction;

namespace MultiChainDotNet.Fluent.Builders2
{
	public sealed class TransactionSender : IAddSignerBuilder, ISignBuilder
	{
		private readonly ILogger _logger;
		private IList<SignerBase> _signers = new List<SignerBase>();
		protected MultiChainTransactionCommand _txnCmd;
		private byte[] _rawSendFrom;
		private string _signed;
		private string _raw;

		public TransactionSender(ILogger<MultiChainFluentApi> logger, MultiChainTransactionCommand txnCmd, string raw)
		{
			_txnCmd = txnCmd;
			_logger = logger;
			_raw = raw;
		}

		public IAddSignerBuilder AddSigner(SignerBase signer)
		{
			_signers.Add(signer);
			return this;
		}

		public ISignBuilder Sign()
		{
			_rawSendFrom = _raw.Hex2Bytes();
			_logger.LogDebug($"InitTransaction:{_rawSendFrom.Bytes2Hex()}");
			_signed = Task.Run(async () =>
			{
				return await SignAsync(_signers[0], _rawSendFrom);
			}).GetAwaiter().GetResult();
			return this;
		}

		//public IAddSignerBuilder Sign(string raw)
		//{
		//	_rawSendFrom = raw.Hex2Bytes();
		//	_logger.LogDebug($"InitTransaction:{_rawSendFrom.Bytes2Hex()}");
		//	_signed = Task.Run(async () =>
		//	{
		//		return await SignAsync(_signers[0], _rawSendFrom);
		//	}).GetAwaiter().GetResult();
		//	return this;
		//}

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

		/// <summary>        
		/// https://bitcoin.stackexchange.com/questions/3374/how-to-redeem-a-basic-tx
		/// 
		/// The input segment of a transaction looks like this:
		///     - version - 4-bytes version field (8 hexadecimal char) 
		///     - vin - 1-byte txin count (2 hexadecimal char)
		///     For each Txin
		///         - prev_txid - 32-bytes previous transaction hash
		///         - prev_vout - 4-bytes index position of the previous transaction's txout
		///         - scriptsig_len -  1-byte variant denoting the length of the prev txn scriptpubkey.
		///         - scriptsig - a copy of the prev txn scriptpubkey as placeholder
		///		- sequence - 4-bytes 
		///  The scriptsig position is after version(4) + vin_count(1) + vin_count * (prev_txid(32) + prev_txout(4)) + scriptsig_len(2)
		///  For a single txin transaction, the position is at 4 + 1 + 36 = 41 bytes or 82 hexa char.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public async Task<string> SignAsync(SignerBase signer, byte[] rawSendFrom, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			var vin = MultiChainTxnHelper.GetVin(rawSendFrom);
			byte[] tempTxn = null;
			IList<byte[]> finalScriptSigs = new List<byte[]>();

			_logger.LogDebug($"Creating transaction for signing...");
			// Create scriptsig for each input
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				try
				{
					// Create the temporary script sig and insert into the transaction.
					_logger.LogDebug($"Creating temporary scriptsig for input {txinIndex}...");
					var tempScriptSig = await CreateTempScriptSigAsync(rawSendFrom, txinIndex);
					_logger.LogTrace($"Temporary ScriptSig:{tempScriptSig.Bytes2Hex()}");

					_logger.LogDebug($"Creating temporary txn for input {txinIndex}...");
					var scriptSigPos = MultiChainTxnHelper.GetScriptSigPosition(rawSendFrom, txinIndex);
					tempTxn = rawSendFrom.BlockReplace(scriptSigPos, BitCoinConstants.SCRIPTSIG_PLACEHOLDER_LENGTH, tempScriptSig);
					tempTxn = tempTxn.Concat(MultiChainTxnHelper.HashTypeCode((byte)hashType)).ToArray();
					_logger.LogTrace($"TempTransaction:{tempTxn.Bytes2Hex()}");

					// Create transaction hash
					byte[] hash = tempTxn.SHA256().SHA256();
					_logger.LogTrace($"TransactionHash:{hash.Bytes2Hex()}");

					// Sign transaction hash
					_logger.LogDebug($"Signing transaction hash txn for input {txinIndex}...");
					byte[] signature = await signer.SignAsync(hash);
					_logger.LogTrace($"Signature:{signature.Bytes2Hex()}");

					// Create sigscript = sigLen + signature + hashType + pubkeyLen + pubkey
					_logger.LogDebug($"Creating final scriptsig for input {txinIndex}...");
					var pubkey = await signer.GetPublicKeyAsync();
					_logger.LogTrace($"Pubkey:{pubkey}");
					var (scriptSigLen, scriptSig) = CreateFinalScriptSig(signature, tempTxn, (byte)hashType, pubkey.Hex2Bytes());
					scriptSig = scriptSigLen.Concat(scriptSig).ToArray();
					_logger.LogTrace($"Final ScriptSig:{scriptSig.Bytes2Hex()}");

					finalScriptSigs.Add(scriptSig);
				}
				catch(Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw;
				}
			}

			// Replace transaction with final scriptsig for each input
			byte[] finalTxn = rawSendFrom;
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				var finalScriptSig = finalScriptSigs[txinIndex];
				var scriptSigPos = MultiChainTxnHelper.GetScriptSigPosition(finalTxn, txinIndex);
				finalTxn = finalTxn.BlockReplace(scriptSigPos, BitCoinConstants.SCRIPTSIG_PLACEHOLDER_LENGTH, finalScriptSig);
			}
			_logger.LogTrace($"FinalTransaction:{finalTxn.Bytes2Hex()}");

			return finalTxn.Bytes2Hex();
		}

		public async Task<string> SignAsync(SignerBase signer, string rawSendFrom, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			return await SignAsync(signer, rawSendFrom.Hex2Bytes(), hashType);
		}

		/// <summary>
		/// The scriptsig is temporarily created using the scriptpubkey of previous transaction. That is why it is needed
		/// to call MultiChain for the prev txn's scriptpubkey from the txin.
		/// </summary>
		/// <param name="transactionHex"></param>
		/// <param name="scriptPubkey"></param>
		/// <param name="index"></param>
		/// <param name="anyonecanpay"></param>
		/// <returns></returns>
		public async Task<byte[]> CreateTempScriptSigAsync(byte[] rawSendFrom, int txinIndex = 0)
		{
			// Get Prev Txid in Little Endian and pull it from blockchain
			var (prevTxid, prevVout) = MultiChainTxnHelper.GetPrevTxn(rawSendFrom, txinIndex);
			var prevTxidHex = prevTxid.Reverse().ToArray().Bytes2Hex();
			_logger.LogTrace($"PrevTxid:{prevTxidHex}");
			_logger.LogTrace($"PrevVout:{prevVout.Bytes2Hex()}");

			// Extract the scriptPubKey
			var result = await _txnCmd.GetTxOutAsync(prevTxidHex, BitConverter.ToInt32(prevVout));
			if (result.IsError)
				throw result.Exception;
			var txout = result.Result;

			var scriptPubKey = txout.ScriptPubKey.Hex.Hex2Bytes();
			VarInt scriptPubKeyLen = new VarInt().Import(scriptPubKey);

			// Create the scriptsig
			var tempScriptSig = scriptPubKeyLen.Bytes;
			tempScriptSig = tempScriptSig.Concat(scriptPubKey).ToArray();
			_logger.LogTrace($"tempScripSig:{tempScriptSig.Bytes2Hex()}");

			return tempScriptSig;
		}

		/// <summary>
		/// The final scriptsig is created using sig length + signature + hashtype + pubkey len + pubkey
		/// Take note that sig length includes the hashtype that is why its length of signature + 1.
		/// The final scriptsig will be used to replace the temporary scriptsig.
		/// </summary>
		/// <param name="signature"></param>
		/// <param name="tempTxn"></param>
		/// <param name="hashType"></param>
		/// <param name="pubkey"></param>
		/// <returns></returns>
		public (byte[] ScriptSigLength, byte[] ScriptSig) CreateFinalScriptSig(byte[] signature, byte[] tempTxn, byte hashType, byte[] pubkey)
		{
			var signatureWithHashType = signature.Append(hashType).ToArray();

			// Encode the length of the signature as VarInt
			VarInt signatureWithHashTypeLen = new VarInt().Import(signatureWithHashType);

			// Encode the length of public key as VarInt
			VarInt pubkeyLen = new VarInt().Import(pubkey);

			byte[] scriptSig = new byte[] { };
			scriptSig = scriptSig.Concat(signatureWithHashTypeLen.Bytes).ToArray();
			scriptSig = scriptSig.Concat(signatureWithHashType).ToArray();
			scriptSig = scriptSig.Concat(pubkeyLen.Bytes).ToArray();
			scriptSig = scriptSig.Concat(pubkey).ToArray();

			var scriptSigLen = new byte[] { (byte)scriptSig.Length };

			return (scriptSigLen, scriptSig);
		}


	}
}
