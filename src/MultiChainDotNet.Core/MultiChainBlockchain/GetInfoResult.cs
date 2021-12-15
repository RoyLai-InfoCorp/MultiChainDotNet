// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;

namespace MultiChainDotNet.Core.MultiChainBlockchain
{
	public class GetInfoResult
	{
		[JsonProperty("version")]
		public string Version { get; set; }

		[JsonProperty("nodeversion")]
		public string NodeVersion { get; set; }

		[JsonProperty("edition")]
		public string Edition { get; set; }

		[JsonProperty("protocolversion")]
		public UInt16 ProtocolVersion { get; set; }

		[JsonProperty("chainname")]
		public string ChainName { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("protocol")]
		public string Protocol { get; set; }

		[JsonProperty("port")]
		public UInt16 Port { get; set; }

		[JsonProperty("setupblocks")]
		public UInt16 SetupBlocks { get; set; }

		[JsonProperty("nodeaddress")]
		public string NodeAddress { get; set; }

		[JsonProperty("burnaddress")]
		public string BurnAddress { get; set; }

		[JsonProperty("incomingpaused")]
		public bool IncomingPaused { get; set; }

		[JsonProperty("miningpaused")]
		public bool MiningPaused { get; set; }

		[JsonProperty("offchainpaused")]
		public bool OffchainPaused { get; set; }

		[JsonProperty("walletversion")]
		public UInt16 WalletVersion { get; set; }

		[JsonProperty("balance")]
		public UInt64 Balance { get; set; }

		[JsonProperty("walletdbversion")]
		public UInt16 WalletDbVersion { get; set; }

		[JsonProperty("reindex")]
		public string Reindex { get; set; }

		[JsonProperty("blocks")]
		public UInt64 Blocks { get; set; }

		[JsonProperty("timeoffset")]
		public UInt16 TimeOffset { get; set; }

		[JsonProperty("connections")]
		public UInt16 Connections { get; set; }

		[JsonProperty("proxy")]
		public string Proxy { get; set; }

		[JsonProperty("difficulty")]
		public Double Difficulty { get; set; }

		[JsonProperty("testnet")]
		public string Testnet { get; set; }

		[JsonProperty("keypoololdest")]
		public UInt64 KeyPoolOldest { get; set; }

		[JsonProperty("keypoolsize")]
		public UInt16 KeyPoolSize { get; set; }

		[JsonProperty("paytxfee")]
		public UInt64 PayTxFee { get; set; }

		[JsonProperty("relayfee")]
		public UInt64 RelayFee { get; set; }

		[JsonProperty("errors")]
		public string Errors { get; set; }
	}
}
