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
