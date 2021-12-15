// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using NUnit.Framework;
using System.Text.RegularExpressions;

namespace MultiChainDotNet.Tests.UnitTests.Core
{
	[TestFixture]
	public class MultiChainInlineMetaDataTests
	{
		[Test]
		public void Should_extract_metadata()
		{
			string decodedVout = "OP_DUP OP_HASH160 cd9dd5f365b7685bbc0907f1d6da5e632724d1b3 OP_EQUALVERIFY OP_CHECKSIG 73706b6602 OP_DROP 73706b647b69046a736f6e7b691044657374696e6174696f6e436861696e6901691244657374696e6174696f6e4164647265737353692a3078613341356543364143454336416436413932464642316233303836354236413239323941453566387d7d OP_DROP";
			string pattern = @"OP_DROP\s(?'data'[^\s]+)\sOP_DROP$";
			var regex = new Regex(pattern);
			var match = Regex.Match(decodedVout, pattern);
			Assert.That(match.Groups["data"].Value, Is.EqualTo("73706b647b69046a736f6e7b691044657374696e6174696f6e436861696e6901691244657374696e6174696f6e4164647265737353692a3078613341356543364143454336416436413932464642316233303836354236413239323941453566387d7d"));
		}

		[Test]
		public void Should_extract_inline_metadata()
		{
			string decodedVout = "73706b6602 OP_DROP OP_RETURN 7b691044657374696e6174696f6e436861696e6901691244657374696e6174696f6e4164647265737353692a3078613341356543364143454336416436413932464642316233303836354236413239323941453566387d";
			string pattern = @"OP_RETURN\s(?'data'[^\s]+)$";
			var regex = new Regex(pattern);
			var match = Regex.Match(decodedVout, pattern);
			Assert.That(match.Groups["data"].Value, Is.EqualTo("7b691044657374696e6174696f6e436861696e6901691244657374696e6174696f6e4164647265737353692a3078613341356543364143454336416436413932464642316233303836354236413239323941453566387d"));
		}


	}
}
