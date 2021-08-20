// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainStream
{

	public class StreamItem<T>
	{
		public List<string> Keys { get; set; }
		public T Data { get; set; }
		public string Publisher { get; set; }

		public StreamItem<string> GetStringRequestItem() => new StreamItem<string>
		{
			Keys = Keys,
			Data = Convert.ToString(Data),
			Publisher = Publisher
		};

		public StreamItem<object> GetObjectRequestItem() => new StreamItem<object>
		{
			Keys = Keys,
			Data = Data,
			Publisher = Publisher
		};

	}
}
