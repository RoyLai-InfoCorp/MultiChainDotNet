#!/bin/bash

#initialise if chain doesn't exist

if [ ! -d "/root/.multichain/${MC_CHAIN_NAME}" ]; then

	echo initialising ${MC_CHAIN_NAME}

	multichain-util create ${MC_CHAIN_NAME}

	cp /root/multichain.conf /root/.multichain/${MC_CHAIN_NAME}/multichain.conf
	cp /root/params.dat /root/.multichain/${MC_CHAIN_NAME}/params.dat

	cd /root/.multichain/${MC_CHAIN_NAME}
	sed -i "s/chain-name = .*$/chain-name = ${MC_CHAIN_NAME}/" params.dat
	sed -i "s/chain-description = .*$/chain-description = ${MC_CHAIN_NAME}/" params.dat
	sed -i "s/default-network-port = .*$/default-network-port = ${MC_NETWORK_PORT}/" params.dat
	sed -i "s/default-rpc-port = .*$/default-rpc-port = ${MC_RPC_PORT}/" params.dat
	sed -i "s/rpcuser=.*$/rpcuser=${MC_RPC_USER}/" multichain.conf
	sed -i "s/rpcpassword=.*$/rpcpassword=${MC_RPC_PASSWORD}/" multichain.conf
	sed -i "/rpcallowip=.*$/d" multichain.conf
	echo "" >> multichain.conf
	echo "rpcallowip=${MC_RPC_ALLOWIP}" >> multichain.conf

	# Start multichaind in background
	multichaind ${MC_CHAIN_NAME} -initprivkey=${MC_PTEKEY} --explorersupport=2 -daemon

	sleep 5

	# Adding 1 test admin, 5 test users
	create_address() {
		multichain-cli ${MC_CHAIN_NAME} grant $address $permission
		multichain-cli ${MC_CHAIN_NAME} send $address 0
	}

	create_address_with_key() {
		multichain-cli ${MC_CHAIN_NAME} importprivkey $wif
		multichain-cli ${MC_CHAIN_NAME} grant $address $permission
		multichain-cli ${MC_CHAIN_NAME} send $address 0
	}

	# Admin
	wif="V8dbxQ8s7yDSmLzXuKiR5Zs7Jrqz6tooxv6qYvmptRy83RXFV4BBBiff"
	permission="admin,connect,mine,receive,send,create,issue"
	address="12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB"
	create_address_with_key

	# TestUser1
	wif="V6D72NoKndZGSopS1n3qHLFqsV9H4CqZ2HU17HDiJmMvvRubmdkZAR4K"
	permission="receive,send"
	address="1Unpjzmh9TsuRZvVKCQNpqx1eDFkaGC215fpj6"
	create_address_with_key

	# TestUser2
	wif="VBHjPn95taPpaPH6eLW6UWcbC6Ku2y6a7EoKWy1oQemJkGqqJhDa3o6c"
	permission="receive,send"
	address="1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN"
	create_address_with_key

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

	# testasset1
	multichain-cli $MC_CHAIN_NAME issue 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB '{ "name": "openasset", "open": true }' 1000 1
	multichain-cli $MC_CHAIN_NAME issue 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB '{ "name": "closeasset", "open": false }' 2000 1

	# teststream
	multichain-cli $MC_CHAIN_NAME create stream "openstream" true

	# Send the relayers startup fund
	multichain-cli $MC_CHAIN_NAME sendfrom 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 12S7Eg2Gz1ZSdRXqVjzjoSybBV1m9umdZz5nHL 1000000000
	multichain-cli $MC_CHAIN_NAME sendfrom 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 1PPUeMEz3LWdoxQDAj31e9ggDTU7HmWitqLV4X 1000000000
	multichain-cli $MC_CHAIN_NAME sendfrom 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 1Sje9v2fiT7A1C3z6yNFqho4fEtqFAke8Xt9B3 1000000000

	multichain-cli $MC_CHAIN_NAME stop

	echo Restarting seednode..
	
	sleep 10s

fi

# Startup
echo Starting seednode..

# Start explorer
cd /root/multichain-explorer-2
sed -i "s/chain1/${MC_CHAIN_NAME}/" /root/multichain-explorer-2/config.ini
sed -i "s/port=.*$/port=${MC_EXPLORER_PORT}/" /root/multichain-explorer-2/config.ini
python3 -m explorer config.ini daemon

if [[ -n "${MC_PUBLIC_IP}" ]]; then

	# public ip is provided
	echo Starting node with external IP
	multichaind ${MC_CHAIN_NAME} -externalip=${MC_PUBLIC_IP}

else

	# public ip is not provided
	echo Starting node with internal IP
	multichaind ${MC_CHAIN_NAME}

fi

