#!/bin/bash
if [ -z $ReceivingHost ]; then
    ReceivingHost="http://localhost:12028/transaction"
fi

if [ -z $TxQuiet ]; then
    echo $1
fi

# publish only if mined
if [ $2 -ne "-1" ]; then
    #curl -s -X POST $ReceivingHost -H 'Content-Type:application/json' -d ''"$1"''
    curl -s -X POST $ReceivingHost -H 'Content-Type:application/json' -d ''\{"txn":"$1","height":$2\}''
fi



