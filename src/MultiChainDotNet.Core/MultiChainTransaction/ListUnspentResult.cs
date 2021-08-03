﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
    public class ListUnspentResult
    {
		#region Internal Classes
		public class ListUnspentAssetResult
		{
			[JsonProperty("name")]
			public string Name { get; set; }
			[JsonProperty("assetref")]
			public string AssetRef { get; set; }
			[JsonProperty("qty")]
			public Double Qty { get; set; }
		}
		#endregion

		[JsonProperty("txid")]
        public string TxId { get; set; }
		
		[JsonProperty("vout")]
		public UInt16 Vout { get; set; }
		
		[JsonProperty("address")]
		public string Address { get; set; }
		
		[JsonProperty("scriptpubkey")]
		public string ScriptPubKey { get; set; }
		
		[JsonProperty("amount")]
		public UInt64 Amount { get; set; }

		[JsonProperty("confirmations")]
		public UInt64 Confirmations { get; set; }
		
		[JsonProperty("cansend")]
		public bool CanSend { get; set; }
		
		[JsonProperty("spendable")]
		public bool Spendable { get; set; }

		[JsonProperty("assets")]
		public List<ListUnspentAssetResult> Assets { get; set; }

		[JsonProperty("permissions")]
		public List<object> Permissions { get; set; }

		[JsonProperty("data")]
		public List<object> Data { get; set; }

	}
}
