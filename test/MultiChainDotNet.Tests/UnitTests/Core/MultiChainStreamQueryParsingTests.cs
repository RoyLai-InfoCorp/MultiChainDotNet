// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.UnitTests.Core
{
	[TestFixture]
    public class MultiChainStreamQueryParsingTests
    {
		string pattern = @"^FROM\s(?'stream'[^\s]+)(\sWHERE\s(?'type'txid|publisher|key)='(?'where'.+)')?(\s(?'order'DESC|ASC))?(\sPAGE\s(?'page'\d+)\sSIZE\s(?'size'\d+))?";
		[Test]
		public void Test1()
		{
			var selectCmd = "FROM a ASC";
			var match = Regex.Match(selectCmd, pattern);
			Assert.That(match.Groups["order"].Value, Is.EqualTo("ASC"));
		}
        
    }
}
