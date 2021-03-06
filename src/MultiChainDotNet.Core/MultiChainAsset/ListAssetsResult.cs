// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;

namespace MultiChainDotNet.Core.MultiChainAsset
{
	public class ListAssetsResult
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("issuetxid")]
		public string IssueTxId { get; set; }

		[JsonProperty("assetref")]
		public string AssetRef { get; set; }

		[JsonProperty("multiple")]
		public int Multiple { get; set; }

		[JsonProperty("open")]
		public bool Open { get; set; }

		[JsonProperty("restrict")]
		public AssetRestrictionResult Restrict { get; set; }

		[JsonProperty("units")]
		public decimal Units { get; set; }

		[JsonProperty("details")]
		public object Details { get; set; }

		[JsonProperty("issueqty")]
		public double IssueQty { get; set; }

		[JsonProperty("issueraw")]
		public UInt64 IssueRaw { get; set; }

		[JsonProperty("subscribed")]
		public bool Subscribed { get; set; }

		[JsonProperty("fungible")]
		public bool Fungible { get; set; }

	}
}
