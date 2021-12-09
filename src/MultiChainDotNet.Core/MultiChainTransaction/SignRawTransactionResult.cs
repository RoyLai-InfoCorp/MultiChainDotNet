// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
	public class SignRawTransactionResult
	{
		[JsonProperty("hex")]
		public string Hex { get; set; }

		[JsonProperty("complete")]
		public bool Complete { get; set; }
	}
}
