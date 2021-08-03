using Newtonsoft.Json;
using System;

namespace MultiChainDotNet.Core.Utils
{
	public struct AddressData
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("wif")]
		public string Wif { get; set; }

		[JsonProperty("ptekey")]
		public string Ptekey { get; set; }

		[JsonProperty("pubkey")]
		public string Pubkey { get; set; }

	}
}
