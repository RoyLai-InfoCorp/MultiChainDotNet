using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

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
