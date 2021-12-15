// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainAddress
{
	public class AddressResult
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("ismine")]
		public bool IsMine { get; set; }
	}
}
