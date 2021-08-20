// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Base
{
    public class TxOut
    {
		public string Value { get; set; }
		public string ScripPubKeyLen { get; set; }
		public string ScriptPubKey { get; set; }
	}
}
