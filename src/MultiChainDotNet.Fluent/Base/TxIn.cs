// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Base
{
    public class TxIn
    {
        public string PrevTxId { get; set; }
		public string PrevVOut { get; set; }
		public string ScriptSigLen { get; set; }
		public string ScriptSig { get; set; }
		public string Sequence { get; set; }
    }
}
