// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainVariable;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
    public class MultiChainVariableTests : TestBase
	{
		MultiChainVariableCommand _varCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain()
				;
		}

		[SetUp]
		public void SetUp()
		{
			_varCmd = _container.GetRequiredService<MultiChainVariableCommand>();
		}

		[Test]
		public async Task Should_create_variable()
		{
			var varName = Guid.NewGuid().ToString("N").Substring(0, 6);

			// ACT 1
			var createResult = await _varCmd.CreateVariableAsync(varName);
			Assert.That(createResult.IsError, Is.False, createResult.ExceptionMessage);

			// ACT 2
			var setResult = await _varCmd.SetVariableValueAsync(varName, "foo bar");
			Assert.That(setResult.IsError, Is.False, setResult.ExceptionMessage);

			// ACT 3
			var getResult = await _varCmd.GetVariableValueAsync<string>(varName);
			Assert.That(getResult.IsError, Is.False, getResult.ExceptionMessage);
			Assert.That(getResult.Result, Is.EqualTo("foo bar"));
		}


	}
}
