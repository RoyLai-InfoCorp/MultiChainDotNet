using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.UnitTests.Core
{
	/*     Result from 'listunspent'

		 [
			 {
				"txid" : "1c10fff2ccd6a4ecb88e43d98a620286e7c34481f2d1b365b5d8f60d03df6d5a",
				"vout" : 0,
				"address" : "12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB",
				"account" : "",
				"scriptPubKey" : "76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac0c73706b67d00700000000000075",
				"amount" : 0,
				"confirmations" : 18,
				"cansend" : true,
				"spendable" : true,
				"assets" : [
					{
						"name" : "closeasset",
						"assetref" : "4-3566-4124",
						"qty" : 2000
					}
				],
				"permissions" : [
				]
			},
			{
				"txid" : "c1feeb768458ad0704d82d6ffccecbffb3065434d0616a66db7cc17d815b7477",
				"vout" : 0,
				"address" : "12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB",
				"account" : "",
				"scriptPubKey" : "76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac0c73706b67e80300000000000075",
				"amount" : 0,
				"confirmations" : 18,
				"cansend" : true,
				"spendable" : true,
				"assets" : [
					{
						"name" : "openasset",
						"assetref" : "4-3287-65217",
						"qty" : 1000
					}
				],
				"permissions" : [
				]
			},
			{
				"txid" : "47904e8a00dfd1da009ea468072d2b92363aa9cd062c81a3a6a19fdbee0611fd",
				"vout" : 0,
				"address" : "12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB",
				"account" : "",
				"scriptPubKey" : "76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac0c73706b67000000000000000075",
				"amount" : 0,
				"confirmations" : 6,
				"cansend" : true,
				"spendable" : true,
				"assets" : [
					{
						"name" : "3e1e9ba8d2",
						"assetref" : "60-265-36935",
						"qty" : 0
					}
				],
				"permissions" : [
				]
			},
			{
				"txid" : "fd495ae5a4820344e0f3ec3b6fea092138b9d32dec070a7d73aa5adeca5eea33",
				"vout" : 0,
				"address" : "12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB",
				"account" : "",
				"scriptPubKey" : "76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac1c73706b74ed6315f2038f2c44ad3a3713189b3fa90100000000000000752e73706b6c000a046e667431000b20fd1106eedb9fa1a6a3812c06cda93a36922b2d0768a49e00dad1df008a4e904775",
				"amount" : 0,
				"confirmations" : 6,
				"cansend" : true,
				"spendable" : true,
				"assets" : [
					{
						"name" : "3e1e9ba8d2",
						"assetref" : "60-265-36935",
						"token" : "nft1",
						"qty" : 1
					}
				],
				"permissions" : [
				]
			},
			{
				"txid" : "dc976ed1e326700233979fcb449203805599d32bfad395385b9ce7813e9df8d2",
				"vout" : 2,
				"address" : "12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB",
				"account" : "",
				"scriptPubKey" : "76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac",
				"amount" : 0,
				"confirmations" : 22,
				"cansend" : true,
				"spendable" : true,
				"assets" : [
				],
				"permissions" : [
				]
			}
		]
	*/


	public class ParseListUnspentTests
	{
		private string content = "[{'txid':'1c10fff2ccd6a4ecb88e43d98a620286e7c34481f2d1b365b5d8f60d03df6d5a','vout':0,'address':'12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB','account':'','scriptPubKey':'76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac0c73706b67d00700000000000075','amount':0,'confirmations':18,'cansend':true,'spendable':true,'assets':[{'name':'closeasset','assetref':'4-3566-4124','qty':2000}],'permissions':[]},{'txid':'c1feeb768458ad0704d82d6ffccecbffb3065434d0616a66db7cc17d815b7477','vout':0,'address':'12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB','account':'','scriptPubKey':'76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac0c73706b67e80300000000000075','amount':0,'confirmations':18,'cansend':true,'spendable':true,'assets':[{'name':'openasset','assetref':'4-3287-65217','qty':1000}],'permissions':[]},{'txid':'47904e8a00dfd1da009ea468072d2b92363aa9cd062c81a3a6a19fdbee0611fd','vout':0,'address':'12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB','account':'','scriptPubKey':'76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac0c73706b67000000000000000075','amount':0,'confirmations':6,'cansend':true,'spendable':true,'assets':[{'name':'3e1e9ba8d2','assetref':'60-265-36935','qty':0}],'permissions':[]},{'txid':'fd495ae5a4820344e0f3ec3b6fea092138b9d32dec070a7d73aa5adeca5eea33','vout':0,'address':'12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB','account':'','scriptPubKey':'76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac1c73706b74ed6315f2038f2c44ad3a3713189b3fa90100000000000000752e73706b6c000a046e667431000b20fd1106eedb9fa1a6a3812c06cda93a36922b2d0768a49e00dad1df008a4e904775','amount':0,'confirmations':6,'cansend':true,'spendable':true,'assets':[{'name':'3e1e9ba8d2','assetref':'60-265-36935','token':'nft1','qty':1}],'permissions':[]},{'txid':'dc976ed1e326700233979fcb449203805599d32bfad395385b9ce7813e9df8d2','vout':2,'address':'12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB','account':'','scriptPubKey':'76a9140dee8693d58dd6fb03aeabc8123037d9f302867d88ac','amount':0,'confirmations':22,'cansend':true,'spendable':true,'assets':[],'permissions':[]}]";

		// FINDING ASSETS  ---------------------------------------------

		[Test]
		public void should_find_unspents_that_CONTAINS_fungible_and_nonfungible_assets_using_LINQ()
		{
			var jtoken = JToken.Parse(content);
			jtoken.Count().Should().Be(5);

			var unspents = jtoken.Where(x => x["assets"].Count() > 0).ToList();
			Console.WriteLine(unspents.ToJson());
			unspents.Count().Should().Be(4);
		}

		[Test]
		public void should_find_unspents_CONTAINS_fungible_and_nonfungible_assets_using_JSONPATH()
		{
			var jtoken = JToken.Parse(content);
			jtoken.Count().Should().Be(5);

			var unspents = jtoken.SelectTokens("$[?(@.assets[?(@)])]").ToList();
			Console.WriteLine(unspents.ToJson());
			unspents.Count().Should().Be(4);
		}


		[Test]
		public void should_find_unspents_that_DOES_NOT_CONTAIN_fungible_and_nonfungible_assets_using_LINQ()
		{
			var jtoken = JToken.Parse(content);
			jtoken.Count().Should().Be(5);

			var unspents = jtoken.Where(x => x["assets"].Count() == 0).ToList();
			Console.WriteLine(unspents.ToJson());
			unspents.Count().Should().Be(1);
		}

		// FINDING FUNGIBLE ASSETS  ---------------------------------------------

		[Test]
		public void should_find_unspents_that_CONTAINS_only_fungible_assets_using_LINQ()
		{
			var jtoken = JToken.Parse(content);
			jtoken.Count().Should().Be(5);

			var unspents = jtoken.Where(x => 
				x["assets"].Count() > 0 &&	x["assets"].All( y=> 
				y["token"] is null)) 
				;

			Console.WriteLine(unspents.ToJson());
			unspents.Count().Should().Be(3);
		}

		// FINDING NON-FUNGIBLE ASSETS  ---------------------------------------------

		[Test]
		public void should_find_unspents_that_CONTAINS_only_non_fungible_token_using_LINQ()
		{
			var jtoken = JToken.Parse(content);
			jtoken.Count().Should().Be(5);

			var unspents = jtoken.Where(x =>
				x["assets"].Count() > 0 && x["assets"].All(y =>
				y["token"] is { }))
				;

			Console.WriteLine(unspents.ToJson());
			unspents.Count().Should().Be(1);
		}

		[Test]
		public void should_find_unspents_that_CONTAINS_only_non_fungible_token_using_LINQ_by_name()
		{
			var jtoken = JToken.Parse(content);
			jtoken.Count().Should().Be(5);

			var unspents = jtoken.Where(x =>
				x["assets"].Count() > 0 && x["assets"].All(y =>
				y["token"]?.ToString()== "nft1"))
				;

			Console.WriteLine(unspents.ToJson());
			unspents.Count().Should().Be(1);
		}


		[Test]
		public void should_find_unspents_that_CONTAINS_only_non_fungible_token_using_JSONPATH()
		{
			var jtoken = JToken.Parse(content);
			jtoken.Count().Should().Be(5);

			var unspents = jtoken.SelectTokens("$[?(@.assets[?(@.token)])]");

			Console.WriteLine(unspents.ToJson());
			unspents.Count().Should().Be(1);
		}

	}
}
