#!/bin/bash
chain="sennet"
multichaind $chain -initprivkey=V8dbxQ8s7yDSmLzXuKiR5Zs7Jrqz6tooxv6qYvmptRy83RXFV4BBBiff -daemon
sleep 5

# Adding 1 test admin, 5 test users
create_address() {
	multichain-cli $chain importprivkey $wif
	multichain-cli $chain grant $address $permission
	multichain-cli $chain send $address 0
}

# Admin
wif="V8dbxQ8s7yDSmLzXuKiR5Zs7Jrqz6tooxv6qYvmptRy83RXFV4BBBiff"
permission="admin,connect,mine,receive,send,create,issue"
address="12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
create_address

# TestUser1
wif="V6D72NoKndZGSopS1n3qHLFqsV9H4CqZ2HU17HDiJmMvvRubmdkZAR4K"
permission="receive,send"
address="1Unpjzmh9TsuRZvVKCQNpqx1eDFkaGC215fpj6"
create_address

# TestUser2
wif="VBHjPn95taPpaPH6eLW6UWcbC6Ku2y6a7EoKWy1oQemJkGqqJhDa3o6c"
permission="receive,send"
address="1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN"
create_address

# Relayer1
wif="VAHJarN329oHzL5fJjEJMSWoMLXdtfBXyu62D74JaKXtt77Ypc9PNaFz"
permission="admin,receive,send,mine,connect"
address="12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL"
create_address

# Relayer2
wif="V8Ktev3iY49EuEeUyaSaiQ1QU3gmW3M5HV8HNdx7srkR3PSty4KbpXZ4"
permission="admin,receive,send,mine,connect"
address="1PPUeMEz3LWdoxQDAj31e9ggDTU7HmWitqLV4X"
create_address

# Relayer3
wif="V8btCNTAyxH1R9NGHF6PSWR8CTzadPHxpKTp75enouS2GHEyqxAZHVrU"
permission="admin,receive,send,mine,connect"
address="1Sje9v2fiT7A1C3z6yNFqho4fEtqFAke8Xt9B3"
create_address

# testasset1
multichain-cli sennet issue 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB '{ "name": "openasset", "open": true }' 1000 1
multichain-cli sennet issue 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB '{ "name": "closeasset", "open": false }' 2000 1

# teststream
multichain-cli sennet create stream "openstream" true

# Send the relayers startup fund
multichain-cli sennet sendfrom 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL 1000000000
multichain-cli sennet sendfrom 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 1PPUeMEz3LWdoxQDAj31e9ggDTU7HmWitqLV4X 1000000000
multichain-cli sennet sendfrom 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 1Sje9v2fiT7A1C3z6yNFqho4fEtqFAke8Xt9B3 1000000000

multichain-cli $chain stop