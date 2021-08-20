// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Configuration;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.UnitTests.Core
{
	[TestFixture]
	public class WalletAddressTests
	{
		MultiChainConfiguration _mcConfig;

		public WalletAddressTests()
		{
			var configRoot = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appSettings.json")
				.Build();
			_mcConfig = configRoot.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>();
		}

		[Test]
		public void Should_be_able_to_generate_address_from_WIF()
		{
			var wif = _mcConfig.Node.Wif;
			Console.WriteLine("Wif:{0}", wif);

			var ptekey = MultiChainAddressHelper.GetPrivateKeyFromWif(wif, _mcConfig.PrivateKeyVersion);
			Console.WriteLine("Ptekey:{0}", ptekey);
			Assert.That(ptekey, Is.EqualTo(_mcConfig.Node.Ptekey));

			var pubkey = MultiChainAddressHelper.GetPublicKeyFromPrivateKey(ptekey);
			Console.WriteLine("Pubkey:{0}", pubkey);
			Assert.That(pubkey, Is.EqualTo(_mcConfig.Node.Pubkey));

			var address = MultiChainAddressHelper.GetAddressFromPublicKey(pubkey, _mcConfig.AddressPubkeyhashVersion, _mcConfig.AddressChecksumValue);
			Console.WriteLine("Address:{0}", address);
			Assert.That(address, Is.EqualTo(_mcConfig.Node.NodeWallet));
		}

		[Test]
		public void Should_be_able_to_generate_address_from_private_key()
		{
			var ptekey = _mcConfig.Node.Ptekey;
			Console.WriteLine("Ptekey:{0}", ptekey);

			var wif = MultiChainAddressHelper.GetWifFromPrivateKey(ptekey, _mcConfig.PrivateKeyVersion, _mcConfig.AddressChecksumValue);
			Console.WriteLine("Wif:{0}", wif);
			Assert.AreEqual(_mcConfig.Node.Wif, wif);

			var pubkey = MultiChainAddressHelper.GetPublicKeyFromPrivateKey(ptekey);
			Console.WriteLine("Pubkey:{0}", pubkey);
			Assert.That(pubkey, Is.EqualTo(_mcConfig.Node.Pubkey));

			var address = MultiChainAddressHelper.GetAddressFromPublicKey(pubkey, _mcConfig.AddressPubkeyhashVersion, _mcConfig.AddressChecksumValue);
			Console.WriteLine("Address:{0}", address);
			Assert.That(address, Is.EqualTo(_mcConfig.Node.NodeWallet));
		}

		[Test]
		public void Should_be_able_to_generate_32bytes_Base64string_From_address()
		{
			var streamName = MultiChainAddressHelper.Get32BytesNameFromAddress(_mcConfig.Node.NodeWallet, _mcConfig.AddressPubkeyhashVersion);
			Console.WriteLine($"address:{_mcConfig.Node.NodeWallet}");
			Console.WriteLine($"stream:{streamName}");
			Assert.That(streamName.Length, Is.EqualTo(32));
			Assert.That(streamName, Is.EqualTo("De6Gk9WN1vsDrqvIEjA32fMChn19qrCv"));
		}

		[Test]
		public void Can_Create_SeedData()
		{
			Dictionary<string, AddressData> knownAddresses = new Dictionary<string, AddressData>();
			knownAddresses["seednode"] = MultiChainAddressHelper.GenerateNewAddress(
				_mcConfig.AddressPubkeyhashVersion,
				_mcConfig.PrivateKeyVersion,
				_mcConfig.AddressChecksumValue);
			knownAddresses["testuser1"] = MultiChainAddressHelper.GenerateNewAddress(
				_mcConfig.AddressPubkeyhashVersion,
				_mcConfig.PrivateKeyVersion,
				_mcConfig.AddressChecksumValue);
			knownAddresses["testuser2"] = MultiChainAddressHelper.GenerateNewAddress(
				_mcConfig.AddressPubkeyhashVersion,
				_mcConfig.PrivateKeyVersion,
				_mcConfig.AddressChecksumValue);
			knownAddresses["relayer1"] = MultiChainAddressHelper.GenerateNewAddress(
				_mcConfig.AddressPubkeyhashVersion,
				_mcConfig.PrivateKeyVersion,
				_mcConfig.AddressChecksumValue);
			knownAddresses["relayer2"] = MultiChainAddressHelper.GenerateNewAddress(
				_mcConfig.AddressPubkeyhashVersion,
				_mcConfig.PrivateKeyVersion,
				_mcConfig.AddressChecksumValue);
			knownAddresses["relayer3"] = MultiChainAddressHelper.GenerateNewAddress(
				_mcConfig.AddressPubkeyhashVersion,
				_mcConfig.PrivateKeyVersion,
				_mcConfig.AddressChecksumValue);

			Console.WriteLine($"{knownAddresses.ToJson()}");
		}

		[Test]
		public void Can_Create_SeedData_Script()
		{
			var configRoot = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appSettings.json")
				.Build();

			var knownAddresses = configRoot.GetSection("Nodes").Get<Dictionary<string, MultiChainNode>>();
			foreach (var data in knownAddresses.Values)
			{
				var ptekey = MultiChainAddressHelper.GetPrivateKeyFromWif(data.Wif, _mcConfig.PrivateKeyVersion);
				var pubkey = MultiChainAddressHelper.GetPublicKeyFromPrivateKey(ptekey);
				var address = MultiChainAddressHelper.GetAddressFromPublicKey(pubkey, _mcConfig.AddressPubkeyhashVersion, _mcConfig.AddressChecksumValue);
				Assert.That(address, Is.EqualTo(data.NodeWallet));
			}

			Console.WriteLine($"{knownAddresses.ToJson()}");
		}

		[Test]
		public void Can_convert_address_to_32_char_base64()
		{
			var address = "12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL";
			var base64 = MultiChainAddressHelper.Get32BytesNameFromAddress(address,_mcConfig.AddressPubkeyhashVersion);
			Assert.That(base64.Length, Is.EqualTo(32));
			Assert.That(base64, Is.EqualTo("CpoRuzgHpkF1GglbyhZ2Ps+RVoqKVYpb"));
		}

		[Test]
		public void Can_convert_address_to_30_char_base64()
		{
			var address = "12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL";
			var base64 = MultiChainAddressHelper.Get28BytesNameFromAddress(address, _mcConfig.AddressPubkeyhashVersion);
			Assert.That(base64.Length, Is.EqualTo(28));
			Console.WriteLine((new byte[] { 01,00,0,0}).Bytes2Base64());
			Assert.That(base64, Is.EqualTo("CpoRuzgHpkF1GglbyhZ2Ps+RVoo="));
		}


	}
}
