// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamItemTextData
	{
		[JsonProperty("text")]
		public string Text { get; set; }
	}
}
