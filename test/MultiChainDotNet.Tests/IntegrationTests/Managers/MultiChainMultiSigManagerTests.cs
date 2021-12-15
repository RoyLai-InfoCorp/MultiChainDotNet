// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainMultiSigManagerTests : TestCommandFactoryBase
	{
		IMultiChainAssetManager _assetManager;
		IMultiChainTransactionManager _txnManager;
		IMultiChainMultiSigManager _multisSigManager;

		public class TestState
		{
			public Guid Id { get; set; }
			public string StreamName { get; set; }
			public string AssetName { get; set; }
			public string MultiSigAddress { get; set; }
			public string RedeemScript { get; set; }

		}

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services.AddTransient<IMultiChainAssetManager, MultiChainAssetManager>();
			services.AddTransient<IMultiChainTransactionManager, MultiChainTransactionManager>();
			services.AddTransient<IMultiChainAddressManager, MultiChainAddressManager>();
			services.AddTransient<IMultiChainMultiSigManager, MultiChainMultiSigManager>();
		}

		[SetUp]
		public void Setup()
		{
			_assetManager = _container.GetRequiredService<IMultiChainAssetManager>();
			_txnManager = _container.GetRequiredService<IMultiChainTransactionManager>();
			_multisSigManager = _container.GetRequiredService<IMultiChainMultiSigManager>();
		}

		[Test, Order(70)]
		public async Task Should_not_be_able_to_create_transaction_when_multisig_address_has_no_fund()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = addressMgr.CreateMultiSig(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var multisig = multisigResult.Result.Address;
			var redeemScript = multisigResult.Result.RedeemScript;
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Result.Raw < 1000)
			{
				var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
				var issueResult = _assetManager.Issue(multisig, assetName, 10, true);

				// ACT
				var signatureResult = _multisSigManager.CreateSendAssetSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);
				Assert.That(signatureResult.Exception.IsMultiChainException(MultiChainErrorCode.RPC_WALLET_INSUFFICIENT_FUNDS)
					, Is.True);
			}
		}


		[Test, Order(80)]
		public async Task Should_be_able_to_multisign_send_asset_transaction()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = addressMgr.CreateMultiSig(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var multisig = multisigResult.Result.Address;
			var redeemScript = multisigResult.Result.RedeemScript;
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			_assetManager.Issue(multisig, assetName, 10, true);
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Result.Raw < 1000)
			{
				_assetManager.Pay(multisig, 5000);
			}

			// ACT
			var signatureResult = _multisSigManager.CreateSendAssetSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);
			Assert.That(signatureResult.IsError, Is.False, signatureResult.ExceptionMessage);
			var signatureSlip = signatureResult.Result;

			var relayer1Sig = _multisSigManager.SignMultiSig(new DefaultSigner(_relayer1.Ptekey), signatureSlip, redeemScript);
			var relayer2Sig = _multisSigManager.SignMultiSig(new DefaultSigner(_relayer2.Ptekey), signatureSlip, redeemScript);
			var result = _multisSigManager.SendMultiSigAsset(new List<string[]> { relayer1Sig.Result, relayer2Sig.Result }, signatureSlip, redeemScript);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		//[Test]
		//public void TemporaryTest1()
		//{
		//	var signatureSlip = "01000000038668b7ee52339117b94c76d9bf5df8aeade6c93e23499c6413ad5310f80970490000000000ffffffff9950d8f6d72078a9d492cff5eaa176d109c1c7adb81f27fa869b18ff90be328c0000000000ffffffff81955383096d1272e4e786078bb3d43ec860499f17f080d07d6ad4e57aa3efe00000000000ffffffff0300000000000000003776a914b33ef123e5caf001824ca1c0c7214ca903a1e7dc88ac1c73706b6fab1a87f4541881c246cb71008eeb9a951027000000000000750000000000000000fd74020573706b6602756a4d69027b690e5472616e7369744d6573736167657b69094d6573736167654964536920313a66783439533048474376785537735754676434542b3948676b4f6f3d3a31690d5472616e73616374696f6e4964536942307839333537363937366664643662333637336237343761313138303638393835333262666334626463313637356139626430623836323932633634613063643664690a5265736f75726365496453691c66783439533048474376785537735754676434542b3948676b4f6f3d690d5265736f757263654e6f6e63656901690b5265736f75726365517479492710690c52656c61794d6573736167657b69094d6573736167654964536920313a66783439533048474376785537735754676434542b3948676b4f6f3d3a31690d5472616e73616374696f6e49645369423078393335373639373666646436623336373362373437613131383036383938353332626663346264633136373561396264306238363239326336346130636436646907546f6b656e496453692a307837663165336434623431633630616663353465656335393338316465313366626431653039306561691242656e6566696369617279436861696e49644330691242656e6566696369617279416464726573735369263152453732753848504d4257775946444c796a6f4e4a4855517779506b70776b33666332514e69114f726967696e61746f72436861696e4964690169114f726967696e61746f724164647265737353692a30783864613537376363313364343834623230366261366538346336336465366363353130313066626369035174794c0de0b6b3a764000069054e6f6e636569017d7d7d3b0100000000000017a9148c315913a231591fce9cda0ffc1ad593011e91568700000000";
		//	var redeemScript = "522103cbb355bd0f558b892113dff5f45c847ef948219673f784970beb5fc532effe8021039b3e46c0d89ae930bcc8d7b45c7e149d14bd69e45cfaec84baf328779d3813612102ba5946e3e0fa9c1da6ba38bdc9f12c75d7c36572bcda80db6feca1b302671f0853ae";
		//	var signatures = new List<string[]>{new string[]
		//	{
		//		"47304402207ea8ef3f536f805a854fc54e5b7638b65884b8aa75f7c693fabae105fa92a1a30220165a163a38759b4cf15f6ba5fe72ce73c8cc5c611ad2b768cee3e78c3639ea2d01",
		//		"47304402205d2daba708b207ce14c05edc4539de49d27b19513917e27e4deac3590c8447f302206ce1137ea8dfafe5371fa5ea1dfff185d79fef67df44813ae996d22962dc5f0901",
		//		"47304402204dfd5d6e07263f0da99c8f8a4acee2be947823dd623eefa1c547e78ed36f8ecb02207abf0e784571c3f8c1885ce5eec13c4f0d4dc9dfd33f1e4fa44144c1d39035fe01"
		//	}, new string[]
		//	{
		//		"47304402200b976ecb85f21c8163ed2e09506f74036b854984241f52e7218bedf557965dd402206304598cd0cafd96cb295424a910230945514d680f4a244f1a5753cd58b6f05201",
		//		"4730440220581d3f536e52269e96d1abfcec7e308ecfa22f0e1b4c2bf39dffead359f956d40220311cb6e9b43dbf3802f813f5ac679f7f6413f29055553b5770ae8ce8b734103401",
		//		"473044022054692368bf9e4132827579318ff52e118ca1c329b0dc24af2a66d6dc06c32cc4022075c88ace9079c3971ef944603158e50ba9825b72e6c073662f1fb2bbbffeac5b01"
		//	}};

		//	var result = _multisSigManager.SendMultiSigAsset(signatures, signatureSlip, redeemScript);
		//	if (result.IsError)
		//		throw result.Exception;
		//}

		//[Test]
		//public void TemporaryTest2()
		//{
		//	var signatureSlip = "01000000038668b7ee52339117b94c76d9bf5df8aeade6c93e23499c6413ad5310f80970490000000000ffffffff9950d8f6d72078a9d492cff5eaa176d109c1c7adb81f27fa869b18ff90be328c0000000000ffffffff81955383096d1272e4e786078bb3d43ec860499f17f080d07d6ad4e57aa3efe00000000000ffffffff0300000000000000003776a914b33ef123e5caf001824ca1c0c7214ca903a1e7dc88ac1c73706b6fab1a87f4541881c246cb71008eeb9a951027000000000000750000000000000000fd74020573706b6602756a4d69027b690e5472616e7369744d6573736167657b69094d6573736167654964536920313a66783439533048474376785537735754676434542b3948676b4f6f3d3a31690d5472616e73616374696f6e4964536942307839333537363937366664643662333637336237343761313138303638393835333262666334626463313637356139626430623836323932633634613063643664690a5265736f75726365496453691c66783439533048474376785537735754676434542b3948676b4f6f3d690d5265736f757263654e6f6e63656901690b5265736f75726365517479492710690c52656c61794d6573736167657b69094d6573736167654964536920313a66783439533048474376785537735754676434542b3948676b4f6f3d3a31690d5472616e73616374696f6e49645369423078393335373639373666646436623336373362373437613131383036383938353332626663346264633136373561396264306238363239326336346130636436646907546f6b656e496453692a307837663165336434623431633630616663353465656335393338316465313366626431653039306561691242656e6566696369617279436861696e49644330691242656e6566696369617279416464726573735369263152453732753848504d4257775946444c796a6f4e4a4855517779506b70776b33666332514e69114f726967696e61746f72436861696e4964690169114f726967696e61746f724164647265737353692a30783864613537376363313364343834623230366261366538346336336465366363353130313066626369035174794c0de0b6b3a764000069054e6f6e636569017d7d7d3b0100000000000017a9148c315913a231591fce9cda0ffc1ad593011e91568700000000";
		//	var redeemScript = "522103cbb355bd0f558b892113dff5f45c847ef948219673f784970beb5fc532effe8021039b3e46c0d89ae930bcc8d7b45c7e149d14bd69e45cfaec84baf328779d3813612102ba5946e3e0fa9c1da6ba38bdc9f12c75d7c36572bcda80db6feca1b302671f0853ae";
		//	var signatures = new List<string[]>{new string[]
		//	{
		//		"47304402205e6a2efd4ab2f9a68f02ad9333f86fc0d9305730018b21b22682d8c7711dc99e022031cfaaf10c5b20009ba16da9bfed7731df3913efdc590c390ce7ce3f50f4cca501",
		//		"473044022000c72facec3cdb75bbd89c060e4be09c01de3303c62eae4cd52183146601ce8702206828e59a6b78ef771239aba24c96b102d763895b070775cdb120a381435c67ec01",
		//		"473044022041ee2954ed32d22cb3ffa46966249c7e3c939d77f91242af47304dae0cddb7210220432ecb0f74bb0989d1b2f77a185ad9d413495403c78a7e4add98311d2653ee0001"
		//	}, new string[]
		//	{
		//		"47304402203c7d40319d2ecc5073b51d868f7b324a2fc94679b6b6145235e8fe5ab652dbda022010a6a4135ba15ad10741f2cc6558392b469594d1f0f26dead8694f109c87719701",
		//		"473044022058219066e64464c0b6651861477cb1c9d76d1a8d728e1e02cc4bc4084dbdc689022043b3cbf075d7e9e78432248aab9bff82f11394fab4fad36bb6c4574ddbc308dc01",
		//		"47304402207dc9894a9d193b241c344415003db9b3ade0c35d7c124691b69e111a1663073e02207e4ef306db8a45e2612fca0632aaa002c31e93d031871603f0b91a581784e66801"
		//	}};

		//	var result = _multisSigManager.SendMultiSigAsset(signatures, signatureSlip, redeemScript);
		//	if (result.IsError)
		//		throw result.Exception;
		//}


	}
}
