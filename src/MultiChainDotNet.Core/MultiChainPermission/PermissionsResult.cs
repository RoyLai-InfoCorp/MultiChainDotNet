// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainPermission
{
	public class PermissionsResult
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("for")]
		public PermissionDetail For { get; set; }

		[JsonProperty("startblock")]
		public UInt64 StartBlock { get; set; }

		[JsonProperty("endblock")]
		public UInt64 EndBlock { get; set; }

		[JsonProperty("admins")]
		public IList<string> Admins { get; set; }

		[JsonProperty("pending")]
		public IList<PermissionsPendingResult> Pending { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }
	}
}
