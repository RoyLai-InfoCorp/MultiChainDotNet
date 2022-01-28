#!/bin/bash
echo $1

if [ -z $ReceivingBlockNotify ]; then
    ReceivingBlockNotify="http://localhost:12028/block"
fi
curl -s -X POST $ReceivingBlockNotify -H 'Content-Type:application/json' -d ''\{"block":\"$1\"\}''



