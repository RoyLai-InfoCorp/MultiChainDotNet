#!/bin/bash

if [[ -z "${MC_PTEKEY}" ]]; then
	multichaind ${MC_CHAIN_NAME}@${MC_SEED_IP}:${MC_NETWORK_PORT} -daemon
else
	multichaind -initprivkey=${MC_PTEKEY} ${MC_CHAIN_NAME}@${MC_SEED_IP}:${MC_NETWORK_PORT} -daemon
fi

sleep 1
pkill multichaind
sleep 2
cd /root/.multichain/${MC_CHAIN_NAME}
sed -i "s/rpcuser.*$/rpcuser=${MC_RPC_USER}/" multichain.conf
sed -i "s/rpcpassword.*$/rpcpassword=${MC_RPC_PASSWORD}/" multichain.conf
sed -i "/rpcallowip=.*$/d" multichain.conf
echo "" >> multichain.conf
echo "rpcallowip=${MC_RPC_ALLOWIP}" >> multichain.conf
multichaind ${MC_CHAIN_NAME} -reindex -rescan -lockinlinemetadata=0
