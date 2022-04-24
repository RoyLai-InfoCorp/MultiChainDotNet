using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainBlockchain
{
	public class GetPeerInfoResult
	{
		[JsonProperty("id")]
		public UInt64 Id { get; set; }

		[JsonProperty("addr")]
		public string Addr { get; set; }

		[JsonProperty("addrlocal")]
		public string AddrLocal { get; set; }

		[JsonProperty("services")]
		public string Services { get; set; }

		[JsonProperty("lastsend")]
		public UInt64 LastSend { get; set; }

		[JsonProperty("lastrecv")]
		public UInt64 LastRecv { get; set; }

		[JsonProperty("bytessent")]
		public UInt64 BytesSent { get; set; }

		[JsonProperty("bytesrecv")]
		public UInt64 BytesRecv { get; set; }

		[JsonProperty("conntime")]
		public UInt64 ConnTime { get; set; }

		[JsonProperty("pingtime")]
		public Decimal PingTime { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }

		[JsonProperty("subver")]
		public string SubVer { get; set; }

		[JsonProperty("handshakelocal")]
		public string HandshakeLocal { get; set; }

		[JsonProperty("handshake")]
		public string Handshake { get; set; }

		[JsonProperty("inbound")]
		public bool Inbound { get; set; }

		[JsonProperty("encrypted")]
		public bool Encrypted { get; set; }

		[JsonProperty("startingheight")]
		public UInt64 StartingHeight { get; set; }

		[JsonProperty("banscore")]
		public UInt64 BanScore { get; set; }

		[JsonProperty("synched_headers")]
		public UInt64 SynchedHeaders { get; set; }

		[JsonProperty("synched_blocks")]
		public UInt64 SynchedBlocks { get; set; }

		[JsonProperty("inflight")]
		public string[] InFlight { get; set; }

		[JsonProperty("whitelisted")]
		public bool WhiteListed { get; set; }
	}
}
