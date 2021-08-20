// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.Base
{
    public struct MultiChainAddressStruct
    {
		[JsonProperty("wif")]
        public string Wif { get; set; }

		[JsonProperty("ptekey")]
		public string Ptekey { get; set; }
		[JsonProperty("pubkey")]
		public string Pubkey { get; set; }
		[JsonProperty("address")]
		public string Address { get; set; }
    }
}
