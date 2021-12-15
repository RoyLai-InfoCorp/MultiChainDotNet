// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainBlockchain
{
	public class GetBlockResult
	{
		[JsonProperty("hash")]
		public string Hash { get; set; }

		[JsonProperty("miner")]
		public string Miner { get; set; }

		[JsonProperty("confirmations")]
		public UInt16 Confirmations { get; set; }

		[JsonProperty("size")]
		public UInt32 Size { get; set; }

		[JsonProperty("height")]
		public UInt64 Height { get; set; }

		[JsonProperty("version")]
		public UInt16 Version { get; set; }

		[JsonProperty("merkleroot")]
		public string MerkleRoot { get; set; }

		[JsonProperty("tx")]
		public List<string> Transactions { get; set; }

		[JsonProperty("time")]
		public UInt64 Time { get; set; }

		[JsonProperty("nonce")]
		public UInt64 Nonce { get; set; }

		[JsonProperty("bits")]
		public string Bits { get; set; }

		[JsonProperty("difficulty")]
		public Double Difficulty { get; set; }

		[JsonProperty("chainwork")]
		public string ChainWork { get; set; }

		[JsonProperty("previousblockhash")]
		public string PreviousBlockHash { get; set; }

		[JsonProperty("nextblockhash")]
		public string NextBlockHash { get; set; }
	}
}
