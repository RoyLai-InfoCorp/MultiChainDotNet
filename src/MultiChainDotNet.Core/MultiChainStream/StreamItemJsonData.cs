// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamItemJsonData
	{
		[JsonProperty("json")]
		public object Json { get; set; }
	}
}
