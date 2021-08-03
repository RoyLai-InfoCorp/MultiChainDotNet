using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Base
{
	public static class BitCoinConstants
	{
		public static byte[] VERSION_BYTE = { 0x01, 0x00, 0x00, 0x00 };
		public const int VERSION_BYTE_LENGTH = 4;
		public const int INPUT_COUNT_LENGTH = 1;
		public const int TXID_Length = 32;
		public const int TXPOS_Length = 4;
		public const int SCRIPTSIG_PLACEHOLDER_LENGTH = 1;
		public const int OUTPUT_COUNT_LENGTH = 1;
		public const int COIN_LENGTH = 8;
		public const int SEQUENCE_LENGTH = 4;
		public static byte[] SEQUENCE = { 0xff, 0xff, 0xff, 0xff };
		public const int LOCKTIME_LENGTH = 4;
		public static byte[] LOCKTIME = { 0x00, 0x00, 0x00, 0x00 };
		public const int HASH_TYPE_LENGTH = 1;
		public const int HASH_TYPE_CODE_LENGTH = 4;
		public const double SMALLEST_UNIT = 0.00000001;

		public enum HashTypeEnum
		{
			SIGHASH_ALL = 0x01,
			SIGHASH_NONE = 0x02,
			SIGHASH_SINGLE = 0x03,
			SIGHASH_ANYONECANPAY = 0x80,
			SIGHASH_ALL_ANYONECANPAY = 0x81,
			SIGHASH_NONE_ANYONECANPAY = 0x82,
			SIGHASH_SINGLE_ANYONECANPAY = 0x83
		}

		/// <summary>
		/// https://en.bitcoin.it/wiki/Script
		/// </summary>

	}

	public struct BitCoinOpCodesEnum
	{
		public const byte OP_0 = 0x00;              // An empty array of bytes is pushed onto the stack. (This is not a no-op: an item is added to the stack.)
													// 0x01 to 0x4b. The next opcode bytes is data to be pushed onto the stack
		public const byte OP_PUSHDATA1 = 0x4c;      // The next byte contains the number of bytes to be pushed onto the stack.
		public const byte OP_PUSHDATA2 = 0x4d;      // The next two bytes contain the number of bytes to be pushed onto the stack in little endian order.
		public const byte OP_PUSHDATA4 = 0x4e;      // The next four bytes contain the number of bytes to be pushed onto the stack in little endian order.
		public const byte OP_1NEGATE = 0x4f;        // The number -1 is pushed onto the stack.
		public const byte OP_1 = 0x51;              // The number 1 is pushed onto the stack.
													// 0x52 to 0x60. The number in the word name (2-16) is pushed onto the stack.

		public const byte OP_RETURN = 0x6a;         // Marks transaction as invalid. Since bitcoin 0.9, a standard way of attaching extra data to transactions is to add a zero-value output with a scriptPubKey consisting of OP_RETURN followed by data. Such outputs are provably unspendable and specially discarded from storage in the UTXO set, reducing their cost to the network. Since 0.12, standard relay rules allow a single output with OP_RETURN, that contains any sequence of push statements (or OP_RESERVED[1]) after the OP_RETURN provided the total scriptPubKey length is at most 83 bytes.
	}
}
