// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using NUnit.Framework;
using System;
using System.IO;

namespace MultiChainDotNet.Tests.IntegrationTests
{
	[SetUpFixture]
	public class TestStartUpClass
	{
		string _logpath = AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\Logs\\log.log";
		public TestStartUpClass()
		{
		}

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			if (File.Exists(_logpath))
				File.Delete(_logpath);
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
		}
	}
}
