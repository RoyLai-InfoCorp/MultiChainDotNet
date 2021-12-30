using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainToken;
using MultiChainDotNet.Core.MultiChainTransaction;
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
		MultiChainTransactionCommand _txCmd;

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

		private async Task<string> CreateMockTokenIssuer()
		{
			var addressCmd = _container.GetRequiredService<MultiChainAddressCommand>();
			var newIssuer = await addressCmd.GetNewAddressAsync();
			var newIssuerAddr = newIssuer.Result;

			await FundWallet(newIssuerAddr);

			var permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();
			await GrantPermissionFromNode(_relayer1, newIssuerAddr, "issue");
			await GrantPermissionFromNode(_relayer2, newIssuerAddr, "issue");
			await permCmd.WaitUntilPermissionGranted(newIssuerAddr, "issue");

			return newIssuerAddr;
		}

		[Test]
		public async Task Should_be_able_to_issue_nfa()
		{
			var nfaName = RandomName();
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);


			// ACT
			var result = await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);
			result.IsError.Should().BeFalse();
			await ShowDecodedTransaction(result.Result);

			// Cna be found on blockchain
			var info = await _tokenCmd.GetNfaInfo(nfaName);
			Console.WriteLine("Info:" + info.Result.ToJson());
			info.IsError.Should().BeFalse();

			// Can be found in wallet
			var nfas = await _tokenCmd.ListNfa(_testUser1.NodeWallet);
			nfas.Result.Any(x => x.Name == nfaName).Should().BeTrue();
		}

		[Test]
		public async Task Should_be_not_be_able_to_issue_nfa_with_longer_than_32Byte_name()
		{
			var nfaName = Guid.NewGuid().ToString("N")+"X";		// 33 char string

			// ACT
			var result = await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);
			result.Exception.IsMultiChainException(MultiChainErrorCode.RPC_INVALID_PARAMETER).Should().BeTrue();
		}

		[Test]
		public async Task Should_be_able_to_list_nfa()
		{
			var result = await _tokenCmd.ListNfa();
			Console.WriteLine(result.Result.ToJson());

			result = await _tokenCmd.ListNfa(_testUser1.NodeWallet);
			Console.WriteLine(result.Result.ToJson());
		}

		[Test]
		public async Task Should_be_able_to_issue_nfa_from()
		{
			var nfaName = RandomName();
			var newIssuerAddr = await CreateMockTokenIssuer();
			Console.WriteLine("Issuer:" + newIssuerAddr);
			Console.WriteLine("NFA:" + nfaName);

			// ACT
			var result = await _tokenCmd.IssueNfaFromAsync(newIssuerAddr, _testUser2.NodeWallet, nfaName);
			if (result.IsError)
				throw result.Exception;

			result.IsError.Should().BeFalse();
			var _permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();
			await _permCmd.GrantPermissionAsync(_testUser2.NodeWallet, $"{nfaName}.issue");

			// Cna be found on blockchain
			var info = await _tokenCmd.GetNfaInfo(nfaName);
			Console.WriteLine("Info:" + info.Result.ToJson());
			info.IsError.Should().BeFalse();

			// Can be found in wallet
			var nfas = await _tokenCmd.ListNfa(_testUser2.NodeWallet);
			nfas.Result.Any(x => x.Name == nfaName).Should().BeTrue();
		}

		[Test]
		public async Task Should_not_be_able_to_issue_duplicate_nfa()
		{
			var nfaName = RandomName();
			Console.WriteLine(nfaName);

			// ACT
			await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);
			var result2 = await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);

			// ASSERT
			result2.ErrorCode.Should().Be(MultiChainDotNet.Core.Base.MultiChainErrorCode.RPC_DUPLICATE_NAME);
		}

		[Test]
		public async Task Should_throw_error_if_nfa_not_found()
		{
			// ACT
			var info2 = await _tokenCmd.GetNfaInfo("AAA");

			// ASSERT
			info2.ErrorCode.Should().Be(MultiChainDotNet.Core.Base.MultiChainErrorCode.RPC_ENTITY_NOT_FOUND);
		}

		[Test]
		public async Task Should_be_able_to_issue_nft()
		{
			var nfaName = RandomName();
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);
			var wait = await _tokenCmd.WaitUntilNfaIssued(_testUser1.NodeWallet, nfaName);

			// ACT
			var tokenId = "nft1";
			var result = await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, tokenId);
			Console.WriteLine(await ShowDecodedTransaction(result.Result));

			// ASSERT
			result.IsError.Should().BeFalse();
			var nft = await _tokenCmd.ListNftByAddress(_testUser1.NodeWallet, nfaName, tokenId);
			nft.Result[0].NfaName.Should().Be(nfaName);
			nft.Result[0].Token.Should().Be(tokenId);
		}

		[Test]
		public async Task Should_be_able_to_list_tokens_issued_by_nfa()
		{
			var nfaName = RandomName();
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);
			var wait = await _tokenCmd.WaitUntilNfaIssued(_testUser1.NodeWallet, nfaName);

			// ACT
			var result = await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, "nftA");
			wait = await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName, "nftA");

			if (result.IsError) throw result.Exception;
			result = await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, "nftB");
			wait = await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName, "nftB");

			if (result.IsError) throw result.Exception;
			result = await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, "nftC");
			wait = await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName, "nftC");

			if (result.IsError) throw result.Exception;
			var nfts = await _tokenCmd.ListNftByAsset(nfaName);

			// ASSERT
			nfts.IsError.Should().BeFalse(nfts.ExceptionMessage);
			nfts.Result.Count.Should().Be(3);
		}



		[Test]
		public async Task Should_be_able_to_list_tokens_by_owner()
		{
			var nfaName = RandomName();
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);
			var wait = await _tokenCmd.WaitUntilNfaIssued(_testUser1.NodeWallet, nfaName);

			// ACT
			var result = await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, "nftA");
			wait = await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName, "nftA");

			if (result.IsError) throw result.Exception;
			result = await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, "nftB");
			wait = await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName, "nftB");

			if (result.IsError) throw result.Exception;
			result = await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, "nftC");
			wait = await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName, "nftC");

			if (result.IsError) throw result.Exception;
			var nfts = await _tokenCmd.ListNftByAddress(_testUser1.NodeWallet, nfaName);

			// ASSERT
			nfts.IsError.Should().BeFalse();
			nfts.Result.Count.Should().Be(3);
		}


		[Test]
		public async Task Should_be_able_to_issue_token_from()
		{
			var nfaName = RandomName();
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNfaAsync(_testUser1.NodeWallet, nfaName);
			var _permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();
			await _permCmd.GrantPermissionAsync(_testUser1.NodeWallet, $"{nfaName}.issue");
			await FundWallet(_testUser1.NodeWallet);

			// ACT
			var tokenId = "nft1";
			var issue = await _tokenCmd.IssueNftFromAsync(_testUser1.NodeWallet, _testUser1.NodeWallet, nfaName, tokenId);
			if (issue.IsError)
				throw issue.Exception;
			var wait = await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName, tokenId);
			wait.Should().BeTrue();

			// ASSERT
			var bal = await _tokenCmd.GetTokenBalancesAsync(_testUser1.NodeWallet);
			bal.Result[_testUser1.NodeWallet].Where(x => x.NfaName == nfaName).Count().Should().Be(1);
			bal.Result[_testUser1.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");
		}


		[Test]
		public async Task Should_be_able_to_send_token()
		{
			var nfaName = RandomName();
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNfaAsync(_admin.NodeWallet, nfaName);
			await _tokenCmd.WaitUntilNfaIssued(_testUser1.NodeWallet, nfaName);
			var tokenId = "nft1";
			await _tokenCmd.IssueNftAsync(_admin.NodeWallet, nfaName, tokenId);
			await _tokenCmd.WaitUntilNftIssued(_testUser1.NodeWallet, nfaName,tokenId);

			// ACT
			var send = await _tokenCmd.SendNftAsync(_testUser1.NodeWallet, nfaName, tokenId);
			if (send.IsError)
				throw send.Exception;

			// ASSERT
			var bal = await _tokenCmd.GetTokenBalancesAsync(_testUser1.NodeWallet);
			bal.Result[_testUser1.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");

		}

		[Test]
		public async Task Should_be_able_to_send_token_from()
		{
			var nfaName = RandomName();
			Console.WriteLine("Issuer:" + _testUser1.NodeWallet);
			Console.WriteLine("NFA:" + nfaName);
			await _tokenCmd.IssueNfaAsync(_admin.NodeWallet, nfaName);
			var tokenId = "nft1";
			await _tokenCmd.IssueNftAsync(_testUser1.NodeWallet, nfaName, tokenId);
			await FundWallet(_testUser1.NodeWallet);

			// ACT
			var send = await _tokenCmd.SendNftFromAsync(_testUser1.NodeWallet, _testUser2.NodeWallet, nfaName, tokenId);
			if (send.IsError)
				throw send.Exception;

			// ASSERT
			var bal = await _tokenCmd.GetTokenBalancesAsync(_testUser2.NodeWallet);
			bal.Result[_testUser2.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");

		}



	}
}
