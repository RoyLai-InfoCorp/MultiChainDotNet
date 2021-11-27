#!/bin/bash
echo $1
if [ -z $ReceivingHost ]; then
    ReceivingHost="http://localhost:12040/RawTransactions"
fi
curl -s -X POST $ReceivingHost -H 'Content-Type:application/json' -d ''"$1"''



