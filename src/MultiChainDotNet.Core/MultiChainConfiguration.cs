// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Core
{
	public class MultiChainConfiguration
	{
		public string AddressPubkeyhashVersion { get; set; }
		public string AddressScripthashVersion { get; set; }
		public string PrivateKeyVersion { get; set; }
		public string AddressChecksumValue { get; set; }
		public string StreamVersion { get; set; }
		public UInt32 Multiple { get; set; }
		public double MinimumTxnFee { get; set; }
		public double MinimumStorageFee { get; set; }
		public IList<string> ExcludeLogError { get; set; }
		public MultiChainNode Node { get; set; }

		public static MultiChainConfiguration BuildConfig(string chainConfigPath, string nodeConfigPath)
		{
			var nodeConfig = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile(nodeConfigPath)
				.Build();

			var chainConfig = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile(chainConfigPath)
				.Build();

			var config = chainConfig.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>();
			config.Node = nodeConfig.GetSection("MultiChainNode").Get<MultiChainNode>();
			return config;
		}
	}
}
