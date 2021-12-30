// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainAsset
{
	public class GetAssetInfoResult
	{
		public class GetAssetInfoIssuesResult
		{
			[JsonProperty("txid")]
			public string TxId { get; set; }

			[JsonProperty("qty")]
			public double Qty { get; set; }

			[JsonProperty("raw")]
			public UInt64 Raw { get; set; }

			[JsonProperty("token")]
			public string Token { get; set; }

			[JsonProperty("details")]
			public object Details { get; set; }

			[JsonProperty("issuers")]
			public string[] Issuers { get; set; }

		}


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

		[JsonProperty("issueqty")]
		public double IssueQty { get; set; }

		[JsonProperty("issueraw")]
		public UInt64 IssueRaw { get; set; }

		[JsonProperty("restrict")]
		public AssetRestrictionResult AssetRestriction { get; set; }

		[JsonProperty("issues")]
		public IList<GetAssetInfoIssuesResult> Issues { get; set; }

	}
}
