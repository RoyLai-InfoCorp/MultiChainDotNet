// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainAddress
{
	public class CreateMultiSigResult
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("redeemScript")]
		public string RedeemScript { get; set; }
	}
}
