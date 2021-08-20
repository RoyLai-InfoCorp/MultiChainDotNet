// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
    public struct TxIdVoutStruct
    {
		[JsonProperty("txid")]
        public string TxId { get; set; }
		[JsonProperty("vout")]
		public UInt16 Vout { get; set; }
    }
}
