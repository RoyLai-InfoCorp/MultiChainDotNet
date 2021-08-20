using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Fluent.Base;
using MultiChainDotNet.Fluent.Builders;
using MultiChainDotNet.Fluent.Signers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent
{
    public class MultiChainFluent
    {
		protected ILogger _logger;
		protected IList<SignerBase> _signers = new List<SignerBase>();
		protected MultiChainTransactionCommand _txnCmd;
		protected byte[] _rawSendFrom;
		protected string _signed;

		public MultiChainFluent AddLogger(ILogger logger)
		{
			_logger = logger;
			return this;
		}


		#region From
		private string _fromAddress;
		public MultiChainFluent From(string address)
		{
			_fromAddress = address;
			return this;
		}
		#endregion

		#region To

		string _to;

		Dictionary<string, Dictionary<string, object>> _toBuilders = new Dictionary<string, Dictionary<string, object>>();

		public MultiChainFluent To(string address)
		{
			if (_toBuilders.ContainsKey(address))
				throw new Exception("Address TO is already added. ");
			_to = address;
			_toBuilders[_to] = new Dictionary<string, object>();
			return this;
		}

		public MultiChainFluent Pay(double qty)
		{
			_toBuilders[_to][""] = qty;
			return this;
		}

		public MultiChainFluent IssueAsset(UInt64 amt)
		{
			_toBuilders[_to]["issue"] = new { raw = amt };
			return this;
		}

		public MultiChainFluent IssueMoreAsset(string assetName, UInt64 amt)
		{
			_toBuilders[_to]["issuemore"] = new { asset = assetName, raw = amt };
			return this;
		}

		public MultiChainFluent SendAsset(string assetName, double qty)
		{
			_toBuilders[_to][assetName] = qty;
			return this;
		}

		public MultiChainFluent Permit(string permission, string entityName = null)
		{
			_toBuilders[_to]["permissions"] = new Dictionary<string, object> { { "type", permission } };
			if (entityName is { })
				((Dictionary<string, object>)_toBuilders[_to]["permissions"])["for"] = entityName;

			return this;
		}

		public MultiChainFluent Revoke(string permission, string entityName = null)
		{
			_toBuilders[_to]["permissions"] = new Dictionary<string, object> { { "type", permission }, { "startblock", 0 }, { "endblock", 0 } };
			if (entityName is { })
				((Dictionary<string, object>)_toBuilders[_to]["permissions"])["for"] = entityName;

			return this;
		}

		public MultiChainFluent AnnotateJson(object json)
		{
			if (json is { })
				_toBuilders[_to]["data"] = new Dictionary<string, object> { { "json", json } };
			return this;
		}

		public MultiChainFluent AnnotateText(string text)
		{
			if (!String.IsNullOrEmpty(text))
				_toBuilders[_to]["data"] = new Dictionary<string, string> { { "text", text } };
			return this;
		}

		public MultiChainFluent AnnotateBytes(byte[] bytes)
		{
			if (bytes is { })
				_toBuilders[_to]["data"] = new Dictionary<string, string> { { "cache", bytes.Bytes2Hex() } };
			return this;
		}

		public MultiChainFluent Filter(string filterName, bool isApprove)
		{
			_toBuilders[_to][filterName] = new { approve = isApprove };
			return this;
		}

		public MultiChainFluent Filter(string filterName, string streamName, bool isApprove)
		{
			_toBuilders[_to][filterName] = new Dictionary<string, string> { { "approve", isApprove.ToString() }, { "for", streamName } };
			return this;
		}

		public MultiChainFluent UpdateLibrary(string libName, string updateName, bool isApprove)
		{
			_toBuilders[_to][libName] = new Dictionary<string, string> { { "approve", isApprove.ToString() }, { "updatename", updateName } };
			return this;
		}

		#endregion

		#region With

		List<object> _withData = new List<object>();
		public MultiChainFluent With()
		{
			return this;
		}

		public MultiChainFluent DeclareBytes(byte[] bytes)
		{
			if (bytes is { })
				_withData.Add(bytes.Bytes2Hex());
			return this;
		}

		public MultiChainFluent DeclareJson(object json)
		{
			if (json is { })
				_withData.Add(new { json = json });
			return this;
		}

		public MultiChainFluent DeclareText(string text)
		{
			if (!String.IsNullOrEmpty(text))
				_withData.Add(new { text = text });
			return this;
		}

		public MultiChainFluent IssueDetails(string assetName, UInt32 multiple, bool canIssueMore)
		{
			_withData.Add(
				new
				{
					create = "asset",
					name = assetName,
					multiple = multiple,
					open = canIssueMore
				});
			return this;
		}

		public MultiChainFluent IssueDetails(string assetName, UInt32 multiple, bool reissuable, Dictionary<string, object> details)
		{
			_withData.Add(
				new
				{
					create = "asset",
					name = assetName,
					multiple = multiple,
					open = reissuable,
					details = details
				});
			return this;
		}

		public MultiChainFluent IssueMoreDetails(string assetName)
		{
			_withData.Add(
				new
				{
					update = assetName
				});
			return this;
		}
		public MultiChainFluent IssueMoreDetails(string assetName, Dictionary<string, object> details)
		{
			_withData.Add(
				new
				{
					update = assetName,
					details = details
				});
			return this;
		}

		public MultiChainFluent CreateStream(string streamName, bool publicWritable, Dictionary<string, object> details)
		{
			_withData.Add(
				new
				{
					create = "stream",
					name = streamName,
					open = publicWritable,
					details = details
				});
			return this;
		}


		public MultiChainFluent CreateStream(string streamName, bool anyoneCanWrite)
		{
			_withData.Add(
				new
				{
					create = "stream",
					name = streamName,
					open = anyoneCanWrite
				});
			return this;
		}

		public MultiChainFluent PublishJson(string streamName, string key, object json)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "for" , streamName },
					{ "key" , key },
					{ "data", new { json = json } }
				});
			return this;
		}

		public MultiChainFluent PublishJson(string streamName, string[] keys, object json)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "for" , streamName },
					{ "keys" , keys },
					{ "data", new { json = json } }
				});
			return this;
		}


		public MultiChainFluent PublishText(string streamName, string key, string text)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "for" , streamName },
					{ "key" , key },
					{ "data", new { text = text } }
				});
			return this;
		}

		public MultiChainFluent CreateVariable(string variableName, object value)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "create" , "variable" },
					{ "name" , variableName },
					{ "value", value }
				});
			return this;
		}

		public MultiChainFluent UpdateVariable(string variableName, object value)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "update" , variableName },
					{ "value", value }
				});
			return this;
		}

		public enum LibraryUpdateMode { NONE, INSTANT, APPROVE }
		public MultiChainFluent AddJavascript(string scriptName, LibraryUpdateMode mode, string javascript)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "create" , "library" },
					{ "name" , scriptName },
					{ "updatemode" , mode.ToString().ToLower()},
					{ "code", javascript }
				});
			return this;
		}

		public MultiChainFluent UpdateJavascript(string scriptName, string versionName, string javascript)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "update" , scriptName },
					{ "updatename" , versionName },
					{ "code", javascript }
				});
			return this;
		}

		#endregion

		#region Normal Transaction

		public MultiChainFluent UseNormalTransaction(MultiChainTransactionCommand txnCmd)
		{
			_txnCmd = txnCmd;
			return this;
		}


		public MultiChainFluent AddSigner(SignerBase signer)
		{
			_signers.Add(signer);
			return this;
		}

		public MultiChainFluent Sign()
		{
			_logger?.LogDebug($"InitTransaction:{_rawSendFrom.Bytes2Hex()}");
			_signed = Task.Run(async () =>
			{
				return await SignAsync(_signers[0], _rawSendFrom);
			}).GetAwaiter().GetResult();
			return this;
		}

		private async Task<string> SignAsync(SignerBase signer, string rawSendFrom, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			return await SignAsync(signer, rawSendFrom.Hex2Bytes(), hashType);
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
		private async Task<string> SignAsync(SignerBase signer, byte[] rawSendFrom, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			var vin = MultiChainTxnHelper.GetVin(rawSendFrom);
			byte[] tempTxn = null;
			IList<byte[]> finalScriptSigs = new List<byte[]>();

			_logger?.LogDebug($"Creating transaction for signing...");
			// Create scriptsig for each input
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				try
				{
					// Create the temporary script sig and insert into the transaction.
					_logger?.LogDebug($"Creating temporary scriptsig for input {txinIndex}...");
					var tempScriptSig = await CreateTempScriptSigAsync(rawSendFrom, txinIndex);
					_logger?.LogTrace($"Temporary ScriptSig:{tempScriptSig.Bytes2Hex()}");

					_logger?.LogDebug($"Creating temporary txn for input {txinIndex}...");
					var scriptSigPos = MultiChainTxnHelper.GetScriptSigPosition(rawSendFrom, txinIndex);
					tempTxn = rawSendFrom.BlockReplace(scriptSigPos, BitCoinConstants.SCRIPTSIG_PLACEHOLDER_LENGTH, tempScriptSig);
					tempTxn = tempTxn.Concat(MultiChainTxnHelper.HashTypeCode((byte)hashType)).ToArray();
					_logger?.LogTrace($"TempTransaction:{tempTxn.Bytes2Hex()}");

					// Create transaction hash
					byte[] hash = tempTxn.SHA256().SHA256();
					_logger?.LogTrace($"TransactionHash:{hash.Bytes2Hex()}");

					// Sign transaction hash
					_logger?.LogDebug($"Signing transaction hash txn for input {txinIndex}...");
					byte[] signature = await signer.SignAsync(hash);
					_logger?.LogTrace($"Signature:{signature.Bytes2Hex()}");

					// Create sigscript = sigLen + signature + hashType + pubkeyLen + pubkey
					_logger?.LogDebug($"Creating final scriptsig for input {txinIndex}...");
					var pubkey = await signer.GetPublicKeyAsync();
					_logger?.LogTrace($"Pubkey:{pubkey}");
					var (scriptSigLen, scriptSig) = CreateFinalScriptSig(signature, tempTxn, (byte)hashType, pubkey.Hex2Bytes());
					scriptSig = scriptSigLen.Concat(scriptSig).ToArray();
					_logger?.LogTrace($"Final ScriptSig:{scriptSig.Bytes2Hex()}");

					finalScriptSigs.Add(scriptSig);
				}
				catch (Exception ex)
				{
					_logger?.LogWarning(ex.ToString());
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
			_logger?.LogTrace($"FinalTransaction:{finalTxn.Bytes2Hex()}");

			return finalTxn.Bytes2Hex();
		}

		private async Task<byte[]> CreateTempScriptSigAsync(byte[] rawSendFrom, int txinIndex = 0)
		{
			// Get Prev Txid in Little Endian and pull it from blockchain
			var (prevTxid, prevVout) = MultiChainTxnHelper.GetPrevTxn(rawSendFrom, txinIndex);
			var prevTxidHex = prevTxid.Reverse().ToArray().Bytes2Hex();
			_logger?.LogTrace($"PrevTxid:{prevTxidHex}");
			_logger?.LogTrace($"PrevVout:{prevVout.Bytes2Hex()}");

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
			_logger?.LogTrace($"tempScripSig:{tempScriptSig.Bytes2Hex()}");

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
		private (byte[] ScriptSigLength, byte[] ScriptSig) CreateFinalScriptSig(byte[] signature, byte[] tempTxn, byte hashType, byte[] pubkey)
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

		#endregion

		#region Multisig Transaction

		public MultiChainFluent UseMultiSigTransaction(MultiChainTransactionCommand txnCmd)
		{
			_txnCmd = txnCmd;
			return this;
		}

		public MultiChainFluent MultiSign(string redeemScript)
		{
			return MultiSign(_rawSendFrom.Bytes2Hex(), redeemScript);
		}

		public MultiChainFluent CreateMultiSigTransaction()
		{
			_rawSendFrom = CreateRawTransaction().Hex2Bytes();
			return this;
		}

		public MultiChainFluent MultiSign(string raw, string redeemScript)
		{
			_rawSendFrom = raw.Hex2Bytes();
			_logger?.LogDebug($"InitTransaction:{_rawSendFrom.Bytes2Hex()}");
			if (_signers is { } && _signers.Count > 0)
			{
				_signed = Task.Run(async () =>
				{
					return await MultiSignUsingSignersAsync(_signers, _rawSendFrom, redeemScript.Hex2Bytes());
				}).GetAwaiter().GetResult();
			}
			else if (_signersSignatures is { } && _signersSignatures.Count > 0)
			{
				return MultiSignUsingSignatures(raw, redeemScript);
			}

			return this;
		}

		private MultiChainFluent MultiSignUsingSignatures(string raw, string redeemScript)
		{
			var signed = raw.Hex2Bytes();
			var vin = MultiChainTxnHelper.GetVin(signed);
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				byte[] scriptSig = new byte[] { 0x00 };
				foreach (var signatureList in _signersSignatures)
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
		private async Task<string> MultiSignUsingSignersAsync(IList<SignerBase> signers, byte[] rawSendFrom, byte[] redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			var vin = MultiChainTxnHelper.GetVin(rawSendFrom);

			// STEP 1: Create the list of temporary transaction hashes, one for each transaction input.
			_logger?.LogDebug($"Creating transaction for signing...");
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

			_logger?.LogDebug($"Final Transaction: {finalTxn.Bytes2Hex()}");
			_logger?.LogDebug($"Final Transaction Decoded: {MultiChainTxnHelper.Decode(finalTxn)}");
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

			_logger?.LogDebug($"Creating transaction for signing...");

			// Create temp script sig
			VarInt redeemScriptLen = new VarInt().Import(redeemScript);
			var tempScriptSig = redeemScriptLen.Bytes;
			tempScriptSig = tempScriptSig.Concat(redeemScript).ToArray();
			_logger?.LogDebug($"tempScripSig:{tempScriptSig.Bytes2Hex()}");

			// Add temp script sig to each input
			List<byte[]> hashes = new List<byte[]>();
			for (int txinIndex = 0; txinIndex < vin; txinIndex++)
			{
				byte[] tempTxn = rawSendFrom;

				// Add temp script sig to each input
				_logger?.LogDebug($"Update temp scriptsig for input {txinIndex}...");
				var scriptSigPos = MultiChainTxnHelper.GetScriptSigPosition(tempTxn, txinIndex);
				tempTxn = tempTxn.BlockReplace(scriptSigPos, BitCoinConstants.SCRIPTSIG_PLACEHOLDER_LENGTH, tempScriptSig);

				tempTxn = tempTxn.Concat(MultiChainTxnHelper.HashTypeCode((byte)hashType)).ToArray();
				_logger?.LogTrace($"TempTransactionDecoded:{MultiChainTxnHelper.Decode(tempTxn)}");

				byte[] hash = tempTxn.SHA256().SHA256();

				hashes.Add(hash);
			}
			return hashes;
		}

		public string CreateMultiSigTransactionHashes(string raw, string redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL)
		{
			return JsonConvert.SerializeObject(CreateMultiSigTransactionHashes(raw.Hex2Bytes(), redeemScript.Hex2Bytes(), hashType));
		}

		IList<string[]> _signersSignatures = new List<string[]>();
		public MultiChainFluent AddMultiSignatures(List<string[]> signatures)
		{
			_signersSignatures = signatures;
			return this;
		}

		#endregion

		#region Create, Describe and Send Raw Transaction

		public string CreateRawTransaction()
		{
			var (fromAddress, tos, with) = CreateRawSendFrom();
			string request = Task.Run(async () =>
			{
				var result = await _txnCmd.CreateRawSendFromAsync(fromAddress, tos, with);
				if (result.IsError)
					throw result.Exception;
				return result.Result;
			}).GetAwaiter().GetResult();
			return request;
		}

		public MultiChainFluent CreateTransaction()
		{
			_rawSendFrom = CreateRawTransaction().Hex2Bytes();
			return this;
		}

		private (string From, Dictionary<string, Dictionary<string, object>> To, List<object> With) CreateRawSendFrom()
		{
			return (_fromAddress, _toBuilders, _withData);
		}

		public string Describe()
		{
			var (fromAddress, tos, with) = CreateRawSendFrom();
			return $"createrawsendfrom {fromAddress} '{JsonConvert.SerializeObject(tos)}' '{JsonConvert.SerializeObject(with)}'";
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

		#endregion

	}
}
