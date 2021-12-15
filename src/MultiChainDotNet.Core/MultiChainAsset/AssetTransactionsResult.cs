// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainAsset
{
	public partial class AssetTransactionsResult
	{
		[JsonProperty("addresses")]
		public Dictionary<string, Int64> Addresses { get; set; }

		[JsonProperty("items")]
		public object[] Items { get; set; }

		[JsonProperty("data")]
		public object[] Data { get; set; }

		[JsonProperty("confirmations")]
		public long Confirmations { get; set; }

		[JsonProperty("blockhash")]
		public string BlockHash { get; set; }

		[JsonProperty("blockindex")]
		public long BlockIndex { get; set; }

		[JsonProperty("blocktime")]
		public long BlockTime { get; set; }

		[JsonProperty("txid")]
		public string TxId { get; set; }

		[JsonProperty("valid")]
		public bool Valid { get; set; }

		[JsonProperty("time")]
		public long Time { get; set; }

		[JsonProperty("timereceived")]
		public long TimeReceived { get; set; }
	}
}
