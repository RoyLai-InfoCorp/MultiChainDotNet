// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.Base
{
	public struct MultiChainAddressStruct
	{
		[JsonProperty("wif")]
		public string Wif { get; set; }

		[JsonProperty("ptekey")]
		public string Ptekey { get; set; }
		[JsonProperty("pubkey")]
		public string Pubkey { get; set; }
		[JsonProperty("address")]
		public string Address { get; set; }
	}
}
