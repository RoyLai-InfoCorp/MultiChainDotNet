// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.Base
{
	public class MultiChainApiMethod
	{
		[JsonProperty("method")]
		public string Method { get; set; }
		[JsonProperty("chain_name")]
		public string ChainName { get; set; }
		[JsonProperty("params")]
		public object[] Params { get; set; }
	}
}
