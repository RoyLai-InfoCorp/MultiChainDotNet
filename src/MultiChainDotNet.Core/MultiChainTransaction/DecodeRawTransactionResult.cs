// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
    public class DecodeRawTransactionResult
    {
		#region internal classes

		public class ScriptPubKeyResult
		{
			[JsonProperty("asm")]
			public string Asm { get; set; }

			[JsonProperty("hex")]
			public string Hex { get; set; }

			[JsonProperty("reqSigs")]
			public int ReqSigs { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }

			[JsonProperty("addresses")]
			public List<string> Addresses { get; set; }
		}

		public class ScriptSigResult
		{
			[JsonProperty("asm")]
			public string Asm { get; set; }

			[JsonProperty("hex")]
			public string Hex { get; set; }
		}


		public class AssetDecodeResult
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("issuetxid")]
			public string Issuetxid { get; set; }

			[JsonProperty("assetref")]
			public string Assetref { get; set; }

			[JsonProperty("qty")]
			public decimal? Qty { get; set; }

			[JsonProperty("raw")]
			public decimal? Raw { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }
		}


		public class GetTxOutResult
		{
			[JsonProperty("bestblock")]
			public string Bestblock { get; set; }

			[JsonProperty("confirmations")]
			public int Confirmations { get; set; }

			[JsonProperty("value")]
			public double Value { get; set; }

			[JsonProperty("scriptPubKey")]
			public ScriptPubKeyResult ScriptPubKey { get; set; }

			[JsonProperty("version")]
			public int Version { get; set; }

			[JsonProperty("coinbase")]
			public bool Coinbase { get; set; }

			[JsonProperty("assets")]
			public List<AssetDecodeResult> Assets { get; set; }

			[JsonProperty("permissions")]
			public List<object> Permissions { get; set; }
		}

		public class VinResult
		{
			[JsonProperty("txid")]
			public string Txid { get; set; }

			[JsonProperty("vout")]
			public int Vout { get; set; }

			[JsonProperty("scriptSig")]
			public ScriptSigResult ScriptSig { get; set; }

			[JsonProperty("sequence")]
			public long Sequence { get; set; }
		}

		public class VoutResult
		{
			[JsonProperty("value")]
			public double Value { get; set; }

			[JsonProperty("n")]
			public int N { get; set; }

			[JsonProperty("scriptPubKey")]
			public ScriptPubKeyResult ScriptPubKey { get; set; }

			[JsonProperty("assets")]
			public List<AssetDecodeResult> Assets { get; set; }

			[JsonProperty("permissions")]
			public List<object> Permissions { get; set; }

			[JsonProperty("items")]
			public List<object> Items { get; set; }

			[JsonProperty("data")]
			public List<object> Data { get; set; }

		}

		#endregion

		[JsonProperty("txid")]
		public string Txid { get; set; }

		[JsonProperty("version")]
		public int Version { get; set; }

		[JsonProperty("locktime")]
		public int Locktime { get; set; }

		[JsonProperty("vin")]
		public List<VinResult> Vin { get; set; }

		[JsonProperty("vout")]
		public List<VoutResult> Vout { get; set; }


	}
}
