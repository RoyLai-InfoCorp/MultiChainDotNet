// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Managers;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	public class MultiChainVariableManagerTests : TestBase
	{
		IMultiChainVariableManager _varManager;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services
				.AddMultiChain()
				.AddTransient<IMultiChainVariableManager, MultiChainVariableManager>();
		}

		[SetUp]
		public void SetUp()
		{
			_varManager = _container.GetRequiredService<IMultiChainVariableManager>();
		}

		[Test]
		public async Task Should_create_variable()
		{
			var varName = RandomName();

			// ACT
			_varManager.CreateVariable(varName);
			_varManager.SetVariableValue(varName, "foo bar");
			var getResult = await _varManager.GetVariableValueAsync<string>(varName);

			// ASSERT
			Assert.That(getResult, Is.EqualTo("foo bar"));
		}


	}
}
