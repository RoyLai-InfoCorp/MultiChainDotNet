// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainPermission
{
    public class PermissionsPendingResult
    {

		[JsonProperty("startblock")]
		public UInt64 StartBlock { get; set; }

		[JsonProperty("endblock")]
		public UInt64 EndBlock { get; set; }

		[JsonProperty("admins")]
		public IList<string> Admins { get; set; }

		[JsonProperty("required")]
		public UInt16 Required { get; set; }

	}
}
