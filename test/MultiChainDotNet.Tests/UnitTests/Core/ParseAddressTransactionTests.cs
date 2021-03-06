// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Tests.UnitTests.Core
{

	/*
	[
		{
			"balance" : {
				"amount" : -1000000000,
				"assets" : [
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
				"1PPUeMEz3LWdoxQDAj31e9ggDTU7HmWitqLV4X"
			],
			"permissions" : [
			],
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 200,
			"blockhash" : "00a8b16741cbbb44991c99622cef04e03e0c7771531a12e27fd1f554cf29f8e5",
			"blockheight" : 4,
			"blockindex" : 0,
			"blocktime" : 1640414489,
			"txid" : "f76a4e7c61c192e2475047e544cb72ad6703c8de0ee3971be6a31a5c3a01d818",
			"valid" : true,
			"time" : 1640414486,
			"timereceived" : 1640414486
		},
		{
			"balance" : {
				"amount" : -1000000000,
				"assets" : [
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
				"1Sje9v2fiT7A1C3z6yNFqho4fEtqFAke8Xt9B3"
			],
			"permissions" : [
			],
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 200,
			"blockhash" : "00a8b16741cbbb44991c99622cef04e03e0c7771531a12e27fd1f554cf29f8e5",
			"blockheight" : 4,
			"blockindex" : 0,
			"blocktime" : 1640414489,
			"txid" : "151ed6556ae41596369fe221d452b97c6938889072092cce52a61f7c5c79485e",
			"valid" : true,
			"time" : 1640414486,
			"timereceived" : 1640414486
		},
		{
			"balance" : {
				"amount" : 0,
				"assets" : [
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
			],
			"permissions" : [
			],
			"issue" : {
				"name" : "3e1e9ba8d2",
				"assetref" : "60-265-36935",
				"multiple" : 1,
				"units" : 1,
				"open" : true,
				"restrict" : {
					"send" : false,
					"receive" : false,
					"issue" : true
				},
				"fungible" : false,
				"canopen" : false,
				"canclose" : false,
				"totallimit" : null,
				"issuelimit" : null,
				"details" : {
				},
				"qty" : 0,
				"raw" : 0,
				"addresses" : [
					"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
				]
			},
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 144,
			"blockhash" : "00cd2ee48b85c60522889e89a2c24d7c83a4467b5aaab2ff471868321a708221",
			"blockheight" : 60,
			"blockindex" : 0,
			"blocktime" : 1640414777,
			"txid" : "47904e8a00dfd1da009ea468072d2b92363aa9cd062c81a3a6a19fdbee0611fd",
			"valid" : true,
			"time" : 1640414775,
			"timereceived" : 1640414775
		},
		{
			"balance" : {
				"amount" : 0,
				"assets" : [
					{
						"name" : "3e1e9ba8d2",
						"assetref" : "60-265-36935",
						"token" : "nft1",
						"qty" : 1
					}
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
			],
			"permissions" : [
			],
			"issue" : {
				"name" : "3e1e9ba8d2",
				"assetref" : "60-265-36935",
				"token" : "nft1",
				"details" : {
				},
				"qty" : 1,
				"raw" : 1,
				"addresses" : [
					"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
				]
			},
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 144,
			"blockhash" : "00cd2ee48b85c60522889e89a2c24d7c83a4467b5aaab2ff471868321a708221",
			"blockheight" : 60,
			"blockindex" : 0,
			"blocktime" : 1640414777,
			"txid" : "fd495ae5a4820344e0f3ec3b6fea092138b9d32dec070a7d73aa5adeca5eea33",
			"valid" : true,
			"time" : 1640414775,
			"timereceived" : 1640414775
		},
		{
			"balance" : {
				"amount" : 0,
				"assets" : [
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
			],
			"permissions" : [
			],
			"create" : {
				"type" : "stream",
				"name" : "0ffe4ee31e46",
				"streamref" : "81-265-14178",
				"restrict" : {
					"write" : false,
					"read" : false,
					"onchain" : false,
					"offchain" : false
				},
				"details" : {
				}
			},
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 123,
			"blockhash" : "00b9e7e8c8356a33441e37d68ad366899230f0d958cdd95f3286ac322f171793",
			"blockheight" : 81,
			"blockindex" : 0,
			"blocktime" : 1640415046,
			"txid" : "62375801b8a0dca99a549e0ff7941a4dfe2920b221c3a6f9f2bcfb0941179259",
			"valid" : true,
			"time" : 1640415042,
			"timereceived" : 1640415042
		},
		{
			"balance" : {
				"amount" : 0,
				"assets" : [
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
			],
			"permissions" : [
			],
			"items" : [
				{
					"type" : "stream",
					"name" : "0ffe4ee31e46",
					"streamref" : "81-265-14178",
					"publishers" : [
						"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
					],
					"keys" : [
						"8a2ebc354c0f",
						"72a3b2a2d194"
					],
					"offchain" : false,
					"available" : true,
					"data" : "a1b2c3d4"
				}
			],
			"data" : [
			],
			"confirmations" : 102,
			"blockhash" : "0028fa5f9202f2b1cce9c01733149b1397c2cf05e4242cd04efab62b9bc4614f",
			"blockheight" : 102,
			"blockindex" : 0,
			"blocktime" : 1640415201,
			"txid" : "7d1a7ffb8796355118e8445cd7fb467c7e84225eb7b8e26c1ba92a2074ecf966",
			"valid" : true,
			"time" : 1640415198,
			"timereceived" : 1640415198
		},
		{
			"balance" : {
				"amount" : -1000,
				"assets" : [
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
				"1Unpjzmh9TsuRZvVKCQNpqx1eDFkaGC215fpj6"
			],
			"permissions" : [
			],
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 60,
			"blockhash" : "002fd857e1881ca943cdfa71281bee25ede94632d01e031399275f0771fa6245",
			"blockheight" : 144,
			"blockindex" : 0,
			"blocktime" : 1640415908,
			"txid" : "093654abdbbf31a5f51ebce4587d1bf64d40224dd48a6018591961d4cdef6b2b",
			"valid" : true,
			"time" : 1640415905,
			"timereceived" : 1640415905
		},
		{
			"balance" : {
				"amount" : -1000,
				"assets" : [
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
				"1Unpjzmh9TsuRZvVKCQNpqx1eDFkaGC215fpj6"
			],
			"permissions" : [
			],
			"items" : [
			],
			"data" : [
				{
					"json" : {
						"DestinationChain" : 1,
						"DestinationAddress" : "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8"
					}
				}
			],
			"confirmations" : 43,
			"blockhash" : "0046299345a3d97a06db555c319ca47d9eef4c0f56789ec1de74a2435d51a79c",
			"blockheight" : 161,
			"blockindex" : 0,
			"blocktime" : 1640415940,
			"txid" : "4bede7787f335fffce32fe99b0b503e2a8b95d859148090b32c8b55ef98346d0",
			"valid" : true,
			"time" : 1640415940,
			"timereceived" : 1640415940
		},
		{
			"balance" : {
				"amount" : 0,
				"assets" : [
					{
						"name" : "openasset",
						"assetref" : "4-3287-65217",
						"qty" : -10
					}
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
				"1Unpjzmh9TsuRZvVKCQNpqx1eDFkaGC215fpj6"
			],
			"permissions" : [
			],
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 22,
			"blockhash" : "008539dd20b64bab2975f629d83062313281844adbcfd2cf3d707820f96bcc91",
			"blockheight" : 182,
			"blockindex" : 0,
			"blocktime" : 1640476855,
			"txid" : "f65a502a7a99a7e7b5904b83905f2e70113467eba5abb768f7804b5b6bd700ca",
			"valid" : true,
			"time" : 1640476852,
			"timereceived" : 1640476852
		},
		{
			"balance" : {
				"amount" : 0,
				"assets" : [
					{
						"name" : "openasset",
						"assetref" : "4-3287-65217",
						"qty" : 10
					}
				]
			},
			"myaddresses" : [
				"12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
			],
			"addresses" : [
				"1Unpjzmh9TsuRZvVKCQNpqx1eDFkaGC215fpj6"
			],
			"permissions" : [
			],
			"items" : [
			],
			"data" : [
			],
			"confirmations" : 3,
			"blockhash" : "0019f1949672f02baf19b9db78aa0b8c0ae198dbf34f9869ebea1954ab45a4d0",
			"blockheight" : 201,
			"blockindex" : 0,
			"blocktime" : 1640476895,
			"txid" : "1b382915749e03d8aaa7b886d8cec98810bef12af6f31db10acfd88b3084f8d5",
			"valid" : true,
			"time" : 1640476893,
			"timereceived" : 1640476893
		}
	]

	 */
	[TestFixture]
	public class ParseAddressTransactionTests
	{

		[Test]
		public void Can_parse_list_address_transactions_result()
		{
			var content = "{'result':[{'balance':{'amount':0,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':['12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB'],'permissions':[{'for':{'type':'asset','name':'HMIy//PkGbN/27pFwiNMHbORwNI=','assetref':'56-299-21735'},'send':false,'receive':false,'issue':true,'admin':false,'activate':false,'custom':[],'startblock':0,'endblock':4294967295,'timestamp':1627792880,'addresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL']}],'items':[],'data':[],'confirmations':155,'blockhash':'000c6a5e99ae3194351620b107c8f36f213ed621f6102d91e307b9409c377006','blockindex':2,'blocktime':1627792883,'txid':'fb55f84b6b4ed7f288464498ec446e998c7b429079b226f76ebfaac8288f5ac6','valid':true,'time':1627792880,'timereceived':1627792880},{'balance':{'amount':0,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':['12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB'],'permissions':[{'for':{'type':'stream','name':'EthRelayPoint','streamref':'78-299-5720'},'write':true,'read':false,'admin':false,'activate':false,'custom':[],'startblock':0,'endblock':4294967295,'timestamp':1627795000,'addresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL']}],'items':[],'data':[],'confirmations':112,'blockhash':'0063da09db8c0251a8cb14d0c43ad66f5ca5eb1c075c686381d6b08cf8747218','blockindex':1,'blocktime':1627795004,'txid':'5120d66de1c7ceb583c4003289f65397111f6dbe1d8177ea9e4bbf2ab7c3e0a1','valid':true,'time':1627795000,'timereceived':1627795000},{'balance':{'amount':27,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':[],'permissions':[],'items':[],'data':['53504b6246304402200cfc7375a2ca5f78524c736309a45809e6d992ae5f67fa18c437f97887f3d275022017d25c29374a7084dc0208901a83a151bea4484f7033a193b7fc924c69bb2645032103cbb355bd0f558b892113dff5f45c847ef948219673f784970beb5fc532effe80'],'confirmations':112,'generated':true,'blockhash':'0063da09db8c0251a8cb14d0c43ad66f5ca5eb1c075c686381d6b08cf8747218','blockindex':0,'blocktime':1627795004,'txid':'1cc39d026eff78985b27f165dedb659c30bb0b4e311ae0f1142d662b6791084c','valid':true,'time':1627795004,'timereceived':1627795005},{'balance':{'amount':100,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':['12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB'],'permissions':[],'items':[],'data':[],'confirmations':91,'blockhash':'005aead454a1bce1da975e3ddb4300b60919bb335285ca04b58ad38b06383c28','blockindex':1,'blocktime':1627795321,'txid':'28b4a5f0602cad5f8a645935b140057e90f84d433035797b92a1f20f56486a80','valid':true,'time':1627795317,'timereceived':1627795317},{'balance':{'amount':-76,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':[],'permissions':[],'items':[{'type':'stream','name':'EthRelayPoint','streamref':'78-299-5720','publishers':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'keys':['0x4c270fee2e0b97527e3c32f6feb5fa870ab1b6405113ce0fdb3e4c4108576741'],'offchain':false,'available':true,'data':{'json':{'Id':'0x4c270fee2e0b97527e3c32f6feb5fa870ab1b6405113ce0fdb3e4c4108576741','TokenRef':'HMIy//PkGbN/27pFwiNMHbORwNI=','BeneficiaryChainId':0,'BeneficiaryAddress':'1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN','OriginatorChainId':1,'OriginatorAddress':'0x8da577cc13d484b206ba6e84c63de6cc51010fbc','Qty':1000000000000001}}}],'data':[],'confirmations':76,'blockhash':'00b4371b918c9748af31ce5bba9fe1e6ca396ddf27ef6aede62effac1eecdbc9','blockindex':1,'blocktime':1627795346,'txid':'9a8864951f4a7708ea25bbea7d2bfc69ee48065c4a253a3d6e8849bf21aefc0f','valid':true,'time':1627795346,'timereceived':1627795346},{'balance':{'amount':1000,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':['12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB'],'permissions':[],'items':[],'data':[],'confirmations':55,'blockhash':'00f0b12238482bac3b65a609ed31f1a668bc030b45a46cc87c546a6c43d70dd0','blockindex':1,'blocktime':1627797244,'txid':'7e1cedad7a2c36024d8cee1da6f0e1355de219e5c8c82d77e28dc2bea3772992','valid':true,'time':1627797240,'timereceived':1627797240},{'balance':{'amount':-76,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':[],'permissions':[],'items':[{'type':'stream','name':'EthRelayPoint','streamref':'78-299-5720','publishers':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'keys':['0x5f5ec7dc63eb33c47440726dfd4f95aed8a107bc9736c49792d52a31844bdf11'],'offchain':false,'available':true,'data':{'json':{'Id':'0x5f5ec7dc63eb33c47440726dfd4f95aed8a107bc9736c49792d52a31844bdf11','AssetId':'HMIy//PkGbN/27pFwiNMHbORwNI=','BeneficiaryChainId':0,'BeneficiaryAddress':'1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN','OriginatorChainId':1,'OriginatorAddress':'0x8da577cc13d484b206ba6e84c63de6cc51010fbc','Qty':1000000000000001}}}],'data':[],'confirmations':42,'blockhash':'00344205e5f2c6eaa0e19ece11f4bfbb3089a849a93fd8bb8ff00de9efa5e679','blockindex':1,'blocktime':1627797265,'txid':'db1937470e4cc4d582955343d22b3b90aa1a2b0cb0b05259b8b53c6bd8a69203','valid':true,'time':1627797263,'timereceived':1627797263},{'balance':{'amount':-72,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':['1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN'],'permissions':[],'issue':{'name':'HMIy//PkGbN/27pFwiNMHbORwNI=','assetref':'56-299-21735','details':{},'qty':1,'raw':1,'addresses':['1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN']},'items':[],'data':[],'confirmations':42,'blockhash':'00344205e5f2c6eaa0e19ece11f4bfbb3089a849a93fd8bb8ff00de9efa5e679','blockindex':2,'blocktime':1627797265,'txid':'d5f0908d11ae413f3db592faa5795f3c25d605980fc23a34cc10a8cfda17858d','valid':true,'time':1627797263,'timereceived':1627797263},{'balance':{'amount':148,'assets':[]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':[],'permissions':[],'items':[],'data':['53504b62473045022100b3cfba04b6ec45e9682adb78ac4522ab17819fb95c235a846a0584e702f1c65702200bfa1bb1167b674e53f268b60327c6eed2851771c66e092a6a1823ac9b1c6c65032103cbb355bd0f558b892113dff5f45c847ef948219673f784970beb5fc532effe80'],'confirmations':42,'generated':true,'blockhash':'00344205e5f2c6eaa0e19ece11f4bfbb3089a849a93fd8bb8ff00de9efa5e679','blockindex':0,'blocktime':1627797265,'txid':'8e034cefb44df6616a7b30c67eda616f4c21952baffe8f80ea9d36813a9c2c70','valid':true,'time':1627797265,'timereceived':1627797265},{'balance':{'amount':0,'assets':[{'name':'openasset','assetref':'4-5214-23549','qty':1}]},'myaddresses':['12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL'],'addresses':['12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB'],'permissions':[],'items':[],'data':[],'confirmations':21,'blockhash':'007227328d67f620d07ee7e9b640193bc9b9f245707fff46adc464fb82da2d06','blockindex':1,'blocktime':1627799961,'txid':'0a532490e7a531137b4ae15d2216b56e6922d533cdf4ed5fa89c58feb45289a3','valid':true,'time':1627799958,'timereceived':1627799958}],'error':null,'id':null}";
			var jtoken = JToken.Parse(content);
			Console.WriteLine(jtoken.ToString(Formatting.Indented));

			//var result = MultiChainResultParser.ParseMultiChainResult<List<ListAddressTransactionResult>>(content);
			//Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

	}
}
