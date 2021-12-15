// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.MultiChainAsset;
using Newtonsoft.Json;
using System;

namespace MultiChainDotNet.Core.MultiChainToken
{
	public class GetNonfungibleAssetInfoResult
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("issuetxid")]
		public string IssueTxId { get; set; }

		[JsonProperty("assetref")]
		public string AssetRef { get; set; }

		[JsonProperty("multiple")]
		public UInt64 Multiple { get; set; }

		[JsonProperty("units")]
		public double Units { get; set; }

		[JsonProperty("open")]
		public bool Open { get; set; }

		[JsonProperty("restrict")]
		public AssetRestrictionResult AssetRestriction { get; set; }

		[JsonProperty("fungible")]
		public bool Fungible { get; set; }

		[JsonProperty("canopen")]
		public bool CanOpen { get; set; }

		[JsonProperty("canclose")]
		public bool CanClose { get; set; }

		[JsonProperty("totallimit")]
		public UInt64? TotalLimit { get; set; }

		[JsonProperty("issuelimit")]
		public UInt64? IssueLimit { get; set; }

		[JsonProperty("details")]
		public object Details { get; set; }

		[JsonProperty("issuecount")]
		public UInt64 IssueCount { get; set; }

		[JsonProperty("issueqty")]
		public Double IssueQty { get; set; }

		[JsonProperty("issueraw")]
		public UInt64 IssueRaw { get; set; }


	}
}
