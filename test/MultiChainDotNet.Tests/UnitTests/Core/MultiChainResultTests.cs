// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Tests.UnitTests.Core
{
	[TestFixture]
	public class MultiChainResultTests
	{
		public class TestClass1
		{
			[JsonProperty("address")]
			public string Address { get; set; }
			[JsonProperty("ismine")]
			public string IsMine { get; set; }
		}

		/// <summary>
		/// {"result":[{"address":"1KW2qaxAHyNWpjNoNHq98Svisd2AFGnK1HJh4M","ismine":true},
		/// {"result":"22bdc8d1f913af5d06097d3b016b37e157bb3d4fd600fd1db6a16673b7d3496f","error":null,"id":null}
		/// </summary>
		[Test]
		public void Should_return_T_if_expect_T()
		{
			string content = @"{""result"":[{""address"":""1KW2qaxAHyNWpjNoNHq98Svisd2AFGnK1HJh4M"",""ismine"":true},{""address"":""1YG7kPhX5H3YYpfpKTaLNvfKhn7jQwLNJtDJpQ"",""ismine"":false}], ""error"":null,""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<List<TestClass1>>(content);
			Assert.That(result.IsError, Is.False);
			Assert.That(result.Result, Has.Count.EqualTo(2));
		}

		[Test]
		public void Should_return_null_if_expect_VoidType_result_in_json()
		{
			string content = @"{""result"":null,""error"":null,""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<VoidType>(content);
			Assert.That(result.IsError, Is.False);
			Assert.That(result.Result, Is.Null);
		}

		[Test]
		public void Should_return_string_if_expect_string_result_in_json()
		{
			string content = @"{""result"":""1KW2qaxAHyNWpjNoNHq98Svisd2AFGnK1HJh4M"",""error"":null,""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<string>(content);
			Assert.That(result.IsError, Is.False);
			Assert.That(result.Result, Is.EqualTo("1KW2qaxAHyNWpjNoNHq98Svisd2AFGnK1HJh4M"));
		}

		/// <summary>
		/// txid
		/// </summary>
		[Test]
		public void Should_return_string_if_expect_string_value()
		{
			string content = "f71799bb2dabf15cb991b414be522441f7ddb25237100f15fc4d23b17be72a8f";
			var result = MultiChainResultParser.ParseMultiChainResult<string>(content);
			Assert.That(result.IsError, Is.False);
			Assert.That(result.Result, Is.EqualTo("f71799bb2dabf15cb991b414be522441f7ddb25237100f15fc4d23b17be72a8f"));
		}


		[Test]
		public void Should_return_bool_if_expect_bool_result_in_json()
		{
			string content = @"{""result"":true,""error"":null,""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<bool>(content);
			Assert.That(result.IsError, Is.False);
			Assert.That(result.Result, Is.True);
		}

		/// <summary>
		/// lockunspent false
		/// </summary>
		[Test]
		public void Should_return_bool_if_expect_bool_value()
		{
			string content = "true";
			var result = MultiChainResultParser.ParseMultiChainResult<bool>(content);
			Assert.That(result.IsError, Is.False);
			Assert.That(result.Result, Is.True);
		}


		/// <summary>
		/// {"result":null,"error":{"code":-5,"message":"Invalid address"},"id":null}
		/// </summary>
		[Test]
		public void Should_throw_expected_errorcode()
		{
			string content = @"{""result"":null,""error"":{""code"":-5,""message"":""Invalid address""},""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<List<TestClass1>>(content);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_INVALID_ADDRESS_OR_KEY));
		}

		/// <summary>
		/// {"result":null,"error":{"id":-5},"id":null}
		/// </summary>
		[Test]
		public void Should_throw_unexpected_errorstructure()
		{
			string content = @"{""result"":null,""error"":{""id"":-5},""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<List<TestClass1>>(content);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.UNKNOWN_ERROR_CODE));
			Console.WriteLine(result.ExceptionMessage);
		}


		[Test]
		public void Should_throw_invalid_error_schema()
		{
			string content = @"{""result"":null,""error"":""UNKNOWN ERROR"",""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<List<TestClass1>>(content);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.UNKNOWN_ERROR_CODE));
			Console.WriteLine(result.ExceptionMessage);
		}

		[Test]
		public void Should_throw_unexpected_errorcode()
		{
			string content = @"{""result"":null,""error"":{""code"":12345,""message"":""Invalid address""},""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<List<TestClass1>>(content);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.UNKNOWN_ERROR_CODE));
			Console.WriteLine(result.ExceptionMessage);
			Assert.That(result.ExceptionMessage, Contains.Substring(@"{""code"":12345,""message"":""Invalid address""}"));
		}

		[Test]
		public void Should_throw_expected_and_return_type_mismatch()
		{
			string content = @"{""result"":""1KW2qaxAHyNWpjNoNHq98Svisd2AFGnK1HJh4M"",""error"":null,""id"":null}";
			var result = MultiChainResultParser.ParseMultiChainResult<bool>(content);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.UNKNOWN_ERROR_CODE));
			Console.WriteLine(result.ExceptionMessage);
			Assert.That(result.ExceptionMessage, Contains.Substring(@"Expected and return type mistmatch."));
		}


	}
}
