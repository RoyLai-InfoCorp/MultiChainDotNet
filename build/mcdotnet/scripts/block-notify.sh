#!/bin/bash

if [ -z $ReceivingBlockNotify ]; then
    ReceivingBlockNotify="http://localhost:12028/block"
fi

if [ -z $BlocksQuiet ]; then
    echo $1
fi

curl -s -X POST $ReceivingBlockNotify -H 'Content-Type:application/json' -d ''\{"block":\"$1\"\}''



