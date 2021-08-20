// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;

namespace MultiChainDotNet.Core.Utils
{
	public static class ObjectExtensions
	{
		public static string ToJson(this object obj)
		{
			return JsonConvert.SerializeObject(obj,
				Formatting.Indented,
				new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		}

	}
}
