// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamsResult
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("createtxid")]
		public string CreateTxId { get; set; }

		[JsonProperty("streamref")]
		public string StreamRef { get; set; }

		[JsonProperty("restrict")]
		public StreamRestriction Restriction { get; set; }

		[JsonProperty("salted")]
		public bool Salted { get; set; }

		[JsonProperty("details")]
		public object Details { get; set; }

		[JsonProperty("creators")]
		public List<string> Creators { get; set; }

		[JsonProperty("subscribed")]
		public bool Subscribed { get; set; }

		[JsonProperty("publishers")]
		public int Publishers { get; set; }

		[JsonProperty("keys")]
		public int Keys { get; set; }

		[JsonProperty("items")]
		public int Items { get; set; }

		[JsonProperty("confirmed")]
		public int Confirmed { get; set; }

	}
}
