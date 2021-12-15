// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
	public abstract class ListTransactionResultBase
	{
		#region internal class
		public class ListAddressTransactionBalanceResult
		{
			public class ListAddressTransactionBalanceAssetResult
			{
				[JsonProperty("name")]
				public string Name { get; set; }

				[JsonProperty("assetref")]
				public string AssetRef { get; set; }

				[JsonProperty("qty")]
				public double Qty { get; set; }

			}

			[JsonProperty("amount")]
			public Int64 Amount { get; set; }

			[JsonProperty("assets")]
			public List<ListAddressTransactionBalanceAssetResult> Assets { get; set; }
		}

		public class ListAddressTransactionPermissionsResult
		{
			public class ListAddressTransactionPermissionsForResult
			{
				[JsonProperty("type")]
				public string EntityType { get; set; }

				[JsonProperty("name")]
				public string EntityName { get; set; }

				[JsonProperty("streamref")]
				public string StreamRef { get; set; }
			}

			[JsonProperty("for")]
			public ListAddressTransactionPermissionsForResult For { get; set; }

			[JsonProperty("connect")]
			public bool Connect { get; set; }
			[JsonProperty("send")]
			public bool Send { get; set; }
			[JsonProperty("receive")]
			public bool Receive { get; set; }
			[JsonProperty("create")]
			public bool Create { get; set; }
			[JsonProperty("issue")]
			public bool Issue { get; set; }
			[JsonProperty("mine")]
			public bool Mine { get; set; }
			[JsonProperty("admin")]
			public bool Admin { get; set; }
			[JsonProperty("activate")]
			public bool Activate { get; set; }
			[JsonProperty("custom")]
			public List<object> Custom { get; set; }
			[JsonProperty("startblock")]
			public UInt64 StartBlock { get; set; }
			[JsonProperty("endblock")]
			public UInt64 EndBlock { get; set; }
			[JsonProperty("timestamp")]
			public UInt64 TimeStamp { get; set; }
			[JsonProperty("addresses")]
			public List<string> Addresses { get; set; }
		}

		public class ListAddressTransactionItemsResult
		{
			[JsonProperty("type")]
			public string TypeName { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("streamref")]
			public string StreamRef { get; set; }

			[JsonProperty("publishers")]
			public List<string> Publishers { get; set; }

			[JsonProperty("keys")]
			public List<string> Keys { get; set; }

			[JsonProperty("offchain")]
			public bool OffChain { get; set; }

			[JsonProperty("available")]
			public bool Available { get; set; }

			[JsonProperty("data")]
			public object Data { get; set; }

		}

		#endregion


		[JsonProperty("addresses")]
		public List<string> Addresses { get; set; }

		[JsonProperty("items")]
		public List<ListAddressTransactionItemsResult> Items { get; set; }

		[JsonProperty("data")]
		public List<object> Data { get; set; }

		/// <summary>
		/// Note: multichain may return -1 for confirmation so it cannot be uint.
		/// </summary>
		[JsonProperty("confirmations")]
		public Int64 Confirmations { get; set; }

		[JsonProperty("blockhash")]
		public string Blockhash { get; set; }

		[JsonProperty("blockindex")]
		public UInt16 BlockIndex { get; set; }

		[JsonProperty("blocktime")]
		public UInt64 BlockTime { get; set; }

		[JsonProperty("txid")]
		public string TxId { get; set; }

		[JsonProperty("valid")]
		public bool Valid { get; set; }

		[JsonProperty("time")]
		public UInt64 Time { get; set; }

		[JsonProperty("timereceived")]
		public UInt64 TimeReceived { get; set; }
	}
}
