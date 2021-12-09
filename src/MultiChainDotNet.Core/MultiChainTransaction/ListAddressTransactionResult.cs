// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
	public class ListAddressTransactionResult : ListTransactionResultBase
	{

		[JsonProperty("balance")]
		public ListAddressTransactionBalanceResult Balance { get; set; }

		[JsonProperty("myaddresses")]
		public List<string> MyAddresses { get; set; }

		[JsonProperty("permissions")]
		public List<ListAddressTransactionPermissionsResult> Permissions { get; set; }


	}
}
