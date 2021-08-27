// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

namespace MultiChainDotNet.Core.Base
{
	/// <summary>
	/// https://www.multichain.com/developers/api-errors/
	/// </summary>
	public enum MultiChainErrorCode
	{
		// JSON-RPC processing errors
		RPC_INVALID_REQUEST = -32600,           //The JSON sent is not a valid JSON-RPC request object.
		RPC_METHOD_NOT_FOUND = -32601,          //The method specified in the JSON-RPC request is not available in MultiChain.
		RPC_INVALID_PARAMS = -32602,            //The parameters provided in the JSON-RPC request are not valid for the method that was called. In practice, this is not used – see instead RPC_INVALID_PARAMETER below.
		RPC_INTERNAL_ERROR = -32603,            //An internal error occurred while processing the JSON-RPC request (you should report this to us).
		RPC_PARSE_ERROR = -32700,               //The message sent cannot be parsed as a JSON-formatted string.

		// General application errors (from Bitcoin Core)
		RPC_MISC_ERROR = -1,                    // A general language-level exception occurred (you should report this to us).
		RPC_FORBIDDEN_BY_SAFE_MODE = -2,        // MultiChain is in safe mode (not documented), and this command is not allowed in safe mode.
		RPC_TYPE_ERROR = -3,                    // An unexpected type was passed as a parameter.
		RPC_INVALID_ADDRESS_OR_KEY = -5,        // An invalid address, public key or private key was passed as a parameter.
		RPC_OUT_OF_MEMORY = -7,                 // MultiChain ran out of memory while performing the operation.
		RPC_INVALID_PARAMETER = -8,             // An invalid value was passed as a parameter.
		RPC_DATABASE_ERROR = -20,               // An error occurred in MultiChain’s internal database (you should report this to us).
		RPC_DESERIALIZATION_ERROR = -22,        // Error in parsing or validating a data structure that was provided in hexadecimal or base64 format.
		RPC_TRANSACTION_ERROR = -25,            // General error while verifying a submitted transaction or block (for now, if not signed properly).
		RPC_TRANSACTION_REJECTED = -26,         // Transaction or block was rejected by the blockchain network’s rules.
		RPC_TRANSACTION_ALREADY_IN_CHAIN = -27, // Transaction rejected because it is already in the blockchain.
		RPC_IN_WARMUP = -28,                    // MultiChain is still warming up and cannot yet response to this request.

		// General application errors (MultiChain specific)
		RPC_NOT_ALLOWED = -701,                 // The requested action is not permitted for the entity or blockchain.
		RPC_NOT_SUPPORTED = -702,               // Action is not supported under these blockchain parameters or runtime parameters.
		RPC_NOT_SUBSCRIBED = -703,              // This node is not subscribed to the specified asset or stream.
		RPC_INSUFFICIENT_PERMISSIONS = -704,    // Address has insufficient permissions for the operation requested.
		RPC_DUPLICATE_NAME = -705,              // An entity with this name already exists on the blockchain.
		RPC_UNCONFIRMED_ENTITY = -706,          // The referred entity has not yet been confirmed (relevant for deprecated protocols).
		RPC_EXCHANGE_ERROR = -707,              // An invalid exchange transaction was passed to one of the exchange APIs.
		RPC_ENTITY_NOT_FOUND = -708,            // The asset or stream reference in an API request or transaction was not found.
		RPC_WALLET_ADDRESS_NOT_FOUND = -709,    // The address provided was not found in the local node’s wallet.
		RPC_TX_NOT_FOUND = -710,                // The specified transaction (by txid) was not found.
		RPC_BLOCK_NOT_FOUND = -711,             // The specified block (by height or hash) was not found.
		RPC_OUTPUT_NOT_FOUND = -712,            // The specified (unspent) transaction output was not found.
		RPC_OUTPUT_NOT_DATA = -713,             // The specified transaction output does not contain any metadata to retrieve.
		RPC_INPUTS_NOT_MINE = -714,             // None of the inputs in this transaction (to be disabled) belong to this node’s wallet.
		RPC_WALLET_OUTPUT_NOT_FOUND = -715,     // The specified (unspent) transaction output was not found in this node’s wallet.
		RPC_WALLET_NO_UNSPENT_OUTPUTS = -716,   // This node’s wallet contains no unspent outputs for creating a new transaction.
		RPC_GENERAL_FILE_ERROR = -717,          // An error occurred when accessing the file path provided.
		RPC_UPGRADE_REQUIRED = -718,            // This blockchain has been upgraded and requires a more recent version of MultiChain.

		// Peer-to-peer client errors
		RPC_CLIENT_NOT_CONNECTED = -9,          // Operation not allowed because MultiChain is not connected to any other nodes.
		RPC_CLIENT_IN_INITIAL_DOWNLOAD = -10,   // Operation not allowed because MultiChain is still downloading the initial block chain.
		RPC_CLIENT_NODE_ALREADY_ADDED = -23,    // The node requested to be manually added has already been added before.
		RPC_CLIENT_NODE_NOT_ADDED = -24,        // This node was not manually added, so the requested information cannot be provided.

		// Wallet errors
		RPC_WALLET_ERROR = -4,                  // A general error occurred with the node’s wallet (you should report this to us).
		RPC_WALLET_INSUFFICIENT_FUNDS = -6,     // The wallet or address has insufficient funds for this transaction.
		RPC_WALLET_INVALID_ACCOUNT_NAME = -11,  // An invalid account name was specified (note that accounts are deprecated).
		RPC_WALLET_KEYPOOL_RAN_OUT = -12,       // The wallet’s key pool ran out (please report this to us).
		RPC_WALLET_UNLOCK_NEEDED = -13,         // This operation requires the wallet to be unlocked using the walletpassphrase command.
		RPC_WALLET_PASSPHRASE_INCORRECT = -14,  // An incorrect wallet passphrase was provided.
		RPC_WALLET_WRONG_ENC_STATE = -15,       // A command was given in the wrong wallet encryption state.
		RPC_WALLET_ENCRYPTION_FAILED = -16,     // The wallet could not be encypted (you should report this to us).

		// UNDOCUMENTED
		NO_ERROR = 0,
		TXOUT_IS_NULL = -9870,					// gettxout returns null.
		ASSET_BALANCE_NOT_FOUND = -9880,		// Asset balance not found.
		CONFIG_NODE_MISSING = -9897,			// Node configuration is missing
		INTERNAL_ERROR = -9998,
		UNKNOWN_ERROR_CODE = -9999,             // Unclassified
		NON_MULTICHAIN_ERROR_CODE = -99999,     // Unclassified
		RAW_EXCHANGE_INCOMPLETE = -1000,        // Tried submitting a submission in incomplete state.
		ASSET_FOLLOW_ON_SCRIPT_REJECTED = 64    // Probably tried to issue a non-reissuable asset.
	}
}
