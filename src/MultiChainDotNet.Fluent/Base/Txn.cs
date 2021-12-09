// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

namespace MultiChainDotNet.Fluent.Base
{
	public class Txn
	{
		public string Version { get; set; }
		public ushort TxInCount { get; set; }
		public TxIn[] Inputs { get; set; }
		public ushort TxOutCount { get; set; }
		public TxOut[] Outputs { get; set; }
		public string LockTime { get; set; }
	}
}
