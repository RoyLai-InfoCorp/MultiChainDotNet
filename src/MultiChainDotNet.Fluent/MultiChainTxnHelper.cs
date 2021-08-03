using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Fluent.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent
{
    public class MultiChainTxnHelper
    {

		public static byte[] HashTypeCode(byte hashType)
		{
			return new byte[] { hashType, 0x00, 0x00, 0x00 };
		}

		public static byte[] CreatePushDataOpCode(UInt32 len)
		{
			if (len < 0x4c)
				return new byte[] { (byte)len };
			if (len < 0xff)
				return new byte[] { BitCoinOpCodesEnum.OP_PUSHDATA1, (byte)len };
			if (len < 0xffff)
				return new byte[] { BitCoinOpCodesEnum.OP_PUSHDATA2 }.Concat(BitConverter.GetBytes((UInt16)len)).ToArray();
			return new byte[] { BitCoinOpCodesEnum.OP_PUSHDATA4 }.Concat(BitConverter.GetBytes((UInt32)len)).ToArray();
		}

		public static (byte[] Txid, byte[] Vout) GetPrevTxn(byte[] txn, int txinIndex)
		{
			var txidPos = GetTxinPosition(txn, txinIndex);
			var txid = new byte[BitCoinConstants.TXID_Length];
			Buffer.BlockCopy(txn, (int)txidPos, txid, 0, BitCoinConstants.TXID_Length);

			var voutPos = GetTxinPosition(txn, txinIndex) + BitCoinConstants.TXID_Length;
			var vout = new byte[BitCoinConstants.TXPOS_Length];
			Buffer.BlockCopy(txn, (int)voutPos, vout, 0, BitCoinConstants.TXPOS_Length);

			return (txid, vout);
		}

		public static UInt32 GetTxinPosition(byte[] txn, int txinIndex)
		{
			UInt32 pos = 0;
			pos += BitCoinConstants.VERSION_BYTE_LENGTH;
			pos += BitCoinConstants.INPUT_COUNT_LENGTH;

			var vin = GetVin(txn);
			for (int i = 0; i < txinIndex && i < vin; i++)
			{
				pos += BitCoinConstants.TXID_Length;
				pos += BitCoinConstants.TXPOS_Length;

				VarInt vinLen = new VarInt().Import(txn, (int)pos);
				pos += (uint)vinLen.Bytes.Length;
				pos += (uint)vinLen.Value;

				pos += BitCoinConstants.SEQUENCE_LENGTH;
			}
			return pos;
		}

		/// <summary>
		/// Get the position of the nth scriptSig in the transaction byte array.
		/// </summary>
		/// <param name="txinIndex"></param>
		/// <returns></returns>
		public static UInt32 GetScriptSigPosition(byte[] txn, int txinIndex)
		{
			var pos = GetTxinPosition(txn, txinIndex);
			pos += BitCoinConstants.TXID_Length;
			pos += BitCoinConstants.TXPOS_Length;
			return pos;
		}

		public static int GetVinPosition(byte[] txn)
		{
			return BitCoinConstants.VERSION_BYTE_LENGTH;
		}

		public static  byte GetVin(byte[] txn)
		{
			return txn[GetVinPosition(txn)];
		}

		public static string Decode(byte[] raw)
		{
			var txn = new Txn();
			int pos = 0;

			txn.Version = raw.SafeSubarray(pos, BitCoinConstants.VERSION_BYTE_LENGTH).Bytes2Hex();
			pos = BitCoinConstants.VERSION_BYTE_LENGTH;

			txn.TxInCount = raw.SafeSubarray(pos, BitCoinConstants.INPUT_COUNT_LENGTH)[0];
			txn.Inputs = new TxIn[txn.TxInCount];
			pos += BitCoinConstants.INPUT_COUNT_LENGTH;

			for (int i = 0; i < txn.TxInCount; i++)
			{
				var input = new TxIn();
				input.PrevTxId = raw.SafeSubarray(pos, BitCoinConstants.TXID_Length).Reverse().ToArray().Bytes2Hex();
				pos += BitCoinConstants.TXID_Length;

				input.PrevVOut = raw.SafeSubarray(pos, BitCoinConstants.TXPOS_Length).Reverse().ToArray().Bytes2Hex();
				pos += BitCoinConstants.TXPOS_Length;

				VarInt vinLen = new VarInt().Import(raw, (int)pos);
				input.ScriptSigLen = vinLen.Bytes.Bytes2Hex();
				pos += vinLen.Bytes.Length;

				input.ScriptSig = raw.SafeSubarray(pos, (int)vinLen.Value).Bytes2Hex();
				pos += (int)vinLen.Value;

				input.Sequence = raw.SafeSubarray(pos, BitCoinConstants.SEQUENCE_LENGTH).Bytes2Hex();
				pos += BitCoinConstants.SEQUENCE_LENGTH;

				txn.Inputs[i] = input;
			}
			txn.TxOutCount = raw[pos];
			txn.Outputs = new TxOut[txn.TxOutCount];
			pos += BitCoinConstants.OUTPUT_COUNT_LENGTH;

			for (int i =0; i <txn.TxOutCount; i++)
			{
				var output = new TxOut();

				output.Value = raw.SafeSubarray(pos, BitCoinConstants.COIN_LENGTH).Bytes2Hex();
				pos += BitCoinConstants.COIN_LENGTH;

				VarInt voutLen = new VarInt().Import(raw, (int)pos);
				output.ScripPubKeyLen = voutLen.Bytes.Bytes2Hex();
				pos += (int)voutLen.Bytes.Length;

				output.ScriptPubKey = raw.SafeSubarray(pos, (int)voutLen.Value).Bytes2Hex();
				pos += (int)voutLen.Value;

				txn.Outputs[i] = output;
			}

			txn.LockTime = raw.SafeSubarray(pos, BitCoinConstants.LOCKTIME_LENGTH).Bytes2Hex();

			return JsonConvert.SerializeObject(txn, Formatting.Indented);


			//------
			//var sb = new StringBuilder();
			//int pos = 0;
			//sb.AppendLine($"Version:{raw.SafeSubarray(pos, BitCoinConstants.VERSION_BYTE_LENGTH).Bytes2Hex()}");

			//pos = BitCoinConstants.VERSION_BYTE_LENGTH;
			//var vin = raw.SafeSubarray(pos, BitCoinConstants.INPUT_COUNT_LENGTH);
			//sb.AppendLine($"Vin:{vin.Bytes2Hex()}");

			//pos += BitCoinConstants.INPUT_COUNT_LENGTH;
			//for (int i = 0; i < vin[0]; i++)
			//{
			//	var txid = raw.SafeSubarray(pos, BitCoinConstants.TXID_Length);
			//	sb.AppendLine($"Txid{i + 1}:{txid.Bytes2Hex()}");
			//	pos += BitCoinConstants.TXID_Length;

			//	var vout = raw.SafeSubarray(pos, BitCoinConstants.TXPOS_Length);
			//	sb.AppendLine($"Vout{i + 1}:{vout.Bytes2Hex()}");
			//	pos += BitCoinConstants.TXPOS_Length;

			//	VarInt vinLen = new VarInt().Import(raw, (int)pos);
			//	var scriptSigLen = vinLen.Bytes;
			//	sb.AppendLine($"ScriptSigLen{i + 1}:{scriptSigLen.Bytes2Hex()}");
			//	pos += vinLen.Bytes.Length;

			//	var scriptSig = raw.SafeSubarray(pos, (int)vinLen.Value);
			//	sb.AppendLine($"ScriptSig{i + 1}:{scriptSig.Bytes2Hex()}");
			//	pos += (int)vinLen.Value;

			//	var sequence = raw.SafeSubarray(pos, BitCoinConstants.SEQUENCE_LENGTH);
			//	sb.AppendLine($"Sequence{i + 1}:{sequence.Bytes2Hex()}");
			//	pos += BitCoinConstants.SEQUENCE_LENGTH;
			//}

			//return sb.ToString();
		}


	}
}
