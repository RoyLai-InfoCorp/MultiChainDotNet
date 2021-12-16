using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainToken;
using MultiChainDotNet.Core.Utils;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
    public class MultiChainTokenTests : TestBase
    {
		MultiChainTokenCommand _tokenCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain();
		}

		[SetUp]
		public async Task SetUp()
		{
			_tokenCmd = _container.GetRequiredService<MultiChainTokenCommand>();
		}

		[Test]
		public async Task Should_be_able_to_issue_nfa()
		{
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 10);
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);


			// ACT
			var result = await _tokenCmd.IssueNonFungibleAssetAsync(_testUser1.NodeWallet, nfaName);
			result.IsError.Should().BeFalse();

			// Cna be found on blockchain
			var info = await _tokenCmd.GetNonfungibleAssetInfo(nfaName);
			Console.WriteLine("Info:"+info.Result.ToJson());
			info.IsError.Should().BeFalse();

			// Can be found in wallet
			var balance = await _tokenCmd.GetAddressBalancesAsync(_testUser1.NodeWallet);
			balance.Result.Any(x=>x.Name == nfaName).Should().BeTrue();
		}

		[Test]
		public async Task Should_be_able_to_issue_nfa_from()
		{
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 10);
			Console.WriteLine("Issuer:"+ _testUser1.NodeWallet);
			Console.WriteLine("NFA:"+nfaName);
			var addressCmd = _container.GetRequiredService<MultiChainAddressCommand>();
			var newIssuer = await addressCmd.GetNewAddressAsync();
			var newIssuerAddr = newIssuer.Result;
			await FundWallet(newIssuerAddr);
			var permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();
			var grant = await permCmd.GrantPermissionFromAsync(_relayer1.NodeWallet, newIssuerAddr, "issue");
			if (grant.IsError) throw grant.Exception;
			grant = await permCmd.GrantPermissionFromAsync(_relayer2.NodeWallet, newIssuerAddr, "issue");
			if (grant.IsError) throw grant.Exception;

			var wait = await TaskHelper.WaitUntilTrue(async () =>
				(await permCmd.CheckPermissionGrantedAsync(newIssuerAddr, "issue")).Result == true
			, 5, 500);
			wait.Should().BeTrue();

			// ACT
			var result = await _tokenCmd.IssueNonFungibleAssetFromAsync(newIssuerAddr, _testUser2.NodeWallet, nfaName);
			if (result.IsError)
				throw result.Exception;

			result.IsError.Should().BeFalse();
			var _permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();
			await _permCmd.GrantPermissionAsync(_testUser2.NodeWallet, $"{nfaName}.issue");

			// Cna be found on blockchain
			var info = await _tokenCmd.GetNonfungibleAssetInfo(nfaName);
			Console.WriteLine("Info:" + info.Result.ToJson());
			info.IsError.Should().BeFalse();

			// Can be found in wallet
			var balance = await _tokenCmd.GetAddressBalancesAsync(_testUser2.NodeWallet);
			balance.Result.Any(x => x.Name == nfaName).Should().BeTrue();
		}


		[Test]
		public async Task Should_not_be_able_to_issue_duplicate_nfa()
		{
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 10);
			Console.WriteLine(nfaName);

			// ACT
			await _tokenCmd.IssueNonFungibleAssetAsync(_testUser1.NodeWallet, nfaName);
			var result2 = await _tokenCmd.IssueNonFungibleAssetAsync(_testUser1.NodeWallet, nfaName);

			// ASSERT
			result2.ErrorCode.Should().Be(MultiChainDotNet.Core.Base.MultiChainErrorCode.RPC_DUPLICATE_NAME);
		}

		[Test]
		public async Task Should_throw_error_if_nfa_not_found()
		{
			// ACT
			var info2 = await _tokenCmd.GetNonfungibleAssetInfo("AAA");

			// ASSERT
			info2.ErrorCode.Should().Be(MultiChainDotNet.Core.Base.MultiChainErrorCode.RPC_ENTITY_NOT_FOUND);
		}

		[Test]
		public async Task Should_be_able_to_issue_token()
		{
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 10);
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNonFungibleAssetAsync( _testUser1.NodeWallet, nfaName);

			// ACT
			var tokenId = "nft1";
			await _tokenCmd.IssueTokenAsync(_testUser1.NodeWallet, nfaName, tokenId);

			// ASSERT
			var bal = await _tokenCmd.GetTokenBalancesAsync(_testUser1.NodeWallet);
			bal.Result[_testUser1.NodeWallet].Where(x => x.NfaName == nfaName).Count().Should().Be(1);
			bal.Result[_testUser1.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");
		}

		[Test]
		public async Task Should_be_able_to_issue_token_from()
		{
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 10);
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNonFungibleAssetAsync(_testUser1.NodeWallet, nfaName);
			var _permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();
			await _permCmd.GrantPermissionAsync(_testUser1.NodeWallet, $"{nfaName}.issue");
			await FundWallet(_testUser1.NodeWallet);

			// ACT
			var tokenId = "nft1";
			var issue = await _tokenCmd.IssueTokenFromAsync(_testUser1.NodeWallet, _testUser1.NodeWallet, nfaName, tokenId);
			if (issue.IsError)
				throw issue.Exception;

			// ASSERT
			var bal = await _tokenCmd.GetTokenBalancesAsync(_testUser1.NodeWallet);
			bal.Result[_testUser1.NodeWallet].Where(x => x.NfaName == nfaName).Count().Should().Be(1);
			bal.Result[_testUser1.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");
		}


		[Test]
		public async Task Should_be_able_to_send_token()
		{
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 10);
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNonFungibleAssetAsync(_admin.NodeWallet, nfaName);
			var tokenId = "nft1";
			await _tokenCmd.IssueTokenAsync(_admin.NodeWallet, nfaName, tokenId);

			// ACT
			var send = await _tokenCmd.SendTokenAsync(_testUser1.NodeWallet, nfaName, tokenId);
			if (send.IsError)
				throw send.Exception;

			// ASSERT
			var bal = await _tokenCmd.GetTokenBalancesAsync(_testUser1.NodeWallet);
			bal.Result[_testUser1.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");

		}

		[Test]
		public async Task Should_be_able_to_send_token_from()
		{
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 10);
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNonFungibleAssetAsync(_admin.NodeWallet, nfaName);
			var tokenId = "nft1";
			await _tokenCmd.IssueTokenAsync(_testUser1.NodeWallet, nfaName, tokenId);
			await FundWallet(_testUser1.NodeWallet);

			// ACT
			var send = await _tokenCmd.SendTokenFromAsync(_testUser1.NodeWallet, _testUser2.NodeWallet, nfaName, tokenId);
			if (send.IsError)
				throw send.Exception;

			// ASSERT
			var bal = await _tokenCmd.GetTokenBalancesAsync(_testUser2.NodeWallet);
			bal.Result[_testUser2.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");

		}



	}
}
