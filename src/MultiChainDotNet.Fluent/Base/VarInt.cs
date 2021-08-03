using MultiChainDotNet.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Base
{
    public class VarInt
    {
		private byte[] _bytes;
		public byte[] Bytes { get { return _bytes; } set { _bytes = value; } }
		public UInt64 Value { get; set; }

		public VarInt()
		{

		}

		public VarInt Import(byte[] txn, int pos)
		{
			var varInt = txn[pos];
			byte[] length = null;

			if (varInt < 0xfd)
			{
				_bytes = new byte[] { varInt };
				Value = varInt;
				return this;
			}

			if (varInt == 0xfd)
			{
				_bytes = new byte[] { 0xfd };
				length = txn.SafeSubarray(pos + 1, 2);
				_bytes = _bytes.Concat(length).ToArray();
				Value = BitConverter.ToUInt16(length);
				return this;
			}
			if (varInt == 0xfe)
			{
				_bytes = new byte[] { 0xfe };
				length = txn.SafeSubarray(pos + 1, 4);
				_bytes = _bytes.Concat(length).ToArray();
				Value= BitConverter.ToUInt32(length);
				return this;
			}

			_bytes = new byte[] { 0xff };
			length = txn.SafeSubarray(pos + 1, 8);
			_bytes = _bytes.Concat(length).ToArray();
			Value = BitConverter.ToUInt64(length);
			return this;
		}

		public VarInt Import(UInt64 length)
		{
			Value = length;
			if (length < 0xfd)
			{
				_bytes = new byte[] { (byte)length };
				return this;
			}

			if (length <= 0xffff)
			{
				_bytes = new byte[] { 0xfd };
				_bytes=_bytes.Concat(BitConverter.GetBytes((UInt16)length)).ToArray();
				return this;
			}
			if (length <= 0xffffffff)
			{
				_bytes = new byte[] { 0xfe };
				_bytes = _bytes.Concat(BitConverter.GetBytes((UInt32)length)).ToArray();
				return this;
			}

			_bytes = new byte[] { 0xff };
			_bytes = _bytes.Concat(BitConverter.GetBytes((UInt64)length)).ToArray();
			return this;
		}
		public VarInt Import(byte[] varData)
		{
			return Import((ulong)varData.Length);
		}

	}
}
