// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainAsset
{
	public class AssetRestrictionResult
	{
		[JsonProperty("send")]
		public bool Send { get; set; }

		[JsonProperty("receive")]
		public bool Receive { get; set; }

		[JsonProperty("issue")]
		public bool Issue { get; set; }
	}
}
