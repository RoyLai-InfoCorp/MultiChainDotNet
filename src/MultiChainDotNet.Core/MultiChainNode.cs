// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using System;

namespace MultiChainDotNet.Core
{
	public class MultiChainNode
	{
		public string NodeName { get; set; }

		/// <summary>
		/// The node wallet is the default wallet address used by the node for signing.
		/// </summary>
		public string NodeWallet { get; set; }

		/// <summary>
		/// Not for production!
		/// </summary>
		public string Pubkey { get; set; }
		/// <summary>
		/// Not for production!
		/// </summary>
		public string Wif { get; set; }
		/// <summary>
		/// Not for production!
		/// </summary>
		public string Ptekey { get; set; }
		public string Protocol { get; set; }
		public string NetworkAddress { get; set; }
		public int NetworkPort { get; set; }
		public string ChainName { get; set; }
		public string Uri => String.Format(@"{0}:\\{1}:{2}\{3}", Protocol, NetworkAddress, NetworkPort, ChainName);
		public string RpcUserName { get; set; }
		public string RpcPassword { get; set; }
	}
}
