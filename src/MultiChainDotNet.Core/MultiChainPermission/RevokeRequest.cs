// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Newtonsoft.Json;
using System.Numerics;

namespace MultiChainDotNet.Core.MultiChainPermission
{
	public class RevokeRequest
	{

		[JsonProperty("for")]
		public string For { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("startblock")]
		public BigInteger? Startblock { get; set; }

		[JsonProperty("endblock")]
		public BigInteger? Endblock { get; set; }

		public RevokeRequest(string assetOrStreamName, string permission)
		{
			For = assetOrStreamName;
			Type = permission;
			Startblock = 0;
			Endblock = 0;
		}

		public RevokeRequest(string permission)
		{
			Type = permission;
			Startblock = 0;
			Endblock = 0;
		}

	}
}
