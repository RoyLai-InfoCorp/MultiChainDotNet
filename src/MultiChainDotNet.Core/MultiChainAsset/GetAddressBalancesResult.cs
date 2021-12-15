// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;

namespace MultiChainDotNet.Core.MultiChainAsset
{
	public class GetAddressBalancesResult
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("assetref")]
		public string AssetRef { get; set; }

		[JsonProperty("qty")]
		public double Qty { get; set; }

		[JsonProperty("raw")]
		public UInt64 Raw { get; set; }
	}
}
