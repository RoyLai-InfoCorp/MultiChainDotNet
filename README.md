# MultiChainDotNet

MultiChainDotNet is a cross-Platform development library for integrating with [MultiChain](https://www.multichain.com/developers/) blockchain platform. It includes features for supporting construction and externally signed transactions for both normal and multisig using a fluent-style syntax. It also features an SQL-style query for stream items . MultiChainDotNet is built using .NET Core on Ubuntu on Windows, aka WSL.

## Features

- The build directory contains docker-compose script used to launch a 4-node multichain network on the development machine running WSL(Ubuntu).
- The Core library contains the wrapper of MultiChain RPC API and included relevant strongly typed class for request and response.
- The Fluent library is a fluent framework for constructing multichain raw transactions and signing transactions externally.
- The Managers library is a high-level abstraction of the underlying classes designed for dependency injection.
- The libraries incorporated some enhancements using HttpClientFactory and Polly to manage the resiliency of calls.
- Uses sql-style command for stream query.
- Web-socket API Listener for transaction monitoring.
- Supports MultiChain version 2.2

## Getting Started

### 1. Install Docker for Windows Desktop with WSL2

https://docs.docker.com/docker-for-windows/wsl/

### 2. Clone this repo

https://github.com/RoyLai-InfoCorp/MultiChainDotNet

## Setup Local Test Network

### 1. Startup 1 seednode and 3 test nodes

Go to /build/docker-compose directory

```bash
sh$ docker-compose up
```

NOTE:

The output should show `Node ready.` for all 4 nodes.

If nodes doesn't start up, press CTRL-C, enter `docker-compose down` and try again.

The test network will take up 12010 to 12019 on the same machine. Make sure these ports are available or change the docker-compose file.

### 2. Test the connection

Each node should be connected to 3 other nodes.

```bash
sh$ docker exec -it mcdotnet-seednode multichain-cli chain1 getpeerinfo
sh$ docker exec -it mcdotnet-relayer1 multichain-cli chain1 getpeerinfo
sh$ docker exec -it mcdotnet-relayer2 multichain-cli chain1 getpeerinfo
```

### 3. Test Explorer

Open browser at localhost:12019. It should show the MultiChain Explorer.

## Compile MultiChainDotNet and Test

1. Install dotnet sdk

   https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu

   For ubuntu 20.04

   ```bash
   wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   rm packages-microsoft-prod.deb
   sudo apt-get update; \
   sudo apt-get install -y apt-transport-https && \
   sudo apt-get update && \
   sudo apt-get install -y dotnet-sdk-5.0
   ```

2. Go to /src directory

   ```bash
   sh$ dotnet build
   ```

3. Go to /test directory

   ```bash
   sh$ dotnet test
   ```

## Developing with MultiChainDotNet

### 1. Using MultiChainConfiguration

Add the configuration to appSettings.json and replace the relevant configuration settings to match the network settings.

```json
    "MultiChainConfiguration": {
        "AddressPubkeyhashVersion": "000d5fec",
        "AddressScripthashVersion": "058a97f8",
        "PrivateKeyVersion": "80b3eab4",
        "AddressChecksumValue": "7cb412e3",
        "Multiple": 1,
        "Node": {
            "Protocol": "http",
            "NodeName": "seednode",
            "NodeWallet": "12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB",
            "Pubkey": "03013ffb59769ea760da19bcc6a22bcb7b0e4a4a1ff64e862916af2703758b8fa0",
            "Wif": "V8dbxQ8s7yDSmLzXuKiR5Zs7Jrqz6tooxv6qYvmptRy83RXFV4BBBiff",
            "Ptekey": "45dea220005d271b35edaf08b84e3e505ebb41886a19980fd585d137be693738",
            "NetworkAddress": "localhost",
            "NetworkPort": 12011,
            "ChainName": "chain1",
            "RpcUserName": "multichainrpc",
            "RpcPassword": "C6n97oxTJrEqmwvVrWGP5TgHpyewRjz2x3soDQKLFkWq"
        }
    }
```

## 2. Write a console app and run getinfo

```C#
    class Program
    {
        static async Task Main(string[] args)
        {
            var container = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services
                    .AddSingleton(hostContext.Configuration.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>())
                    .AddMultiChain()
                    .AddMultiChainManagers()
                    .AddSingleton<SignerBase>(sp => new DefaultSigner(sp.GetRequiredService<MultiChainConfiguration>().Node.Ptekey))
                    ;
                }).Build().Services;
            var config = container.GetRequiredService<IConfiguration>();
            var blockchain = container.GetRequiredService<MultiChainBlockchainCommand>();
            var getinfo = await blockchain.GetInfoAsync();
            Console.WriteLine(JsonConvert.SerializeObject(getinfo.Result,Formatting.Indented));

        }
    }
```

## Using MultiChainDotNet Command

All MultiChainDotNet Command classes follows the [MultiChain RPC API](https://www.multichain.com/developers/json-rpc-api/) command structure. Below example shows how to issue an asset using the MultiChainAssetCommand. It will issue 100 units of "testasset" into the node address from the appSettings.json configuration file and show balance.

```
    var mcConfig = container.GetRequiredService<MultiChainConfiguration>();
    var address = mcConfig.Node.NodeWallet;
    var assetCmd = container.GetRequiredService<MultiChainAssetCommand>();
    await assetCmd.IssueAssetFromAsync(address, address, "testasset", 100, 1, true);
    var getBalance = assetCmd.GetAddressBalancesAsync(address);
    Console.WriteLine(JsonConvert.SerializeObject(getBalance.Result, Formatting.Indented));
```

This is equivalent to sending the API call 'issuefrom 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB {"name":"testasset","open":true} 100 1' assuming that the wallet address is 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB.

## Using MultiChain Fluent

The Fluent library is used to aid the construction, signing and sending of both normal and multisignature raw transactions using a fluent-style syntax. The MultiChainFluent class has 2 main functions. The first function is for constructing the transaction. The second function handles the signing and sending. The SignerBase class can be derived for other forms of signing mechanism such as hardware signing.

The library has also made some changes to the MultiChain terminologies to lend clarity when constructing the transaction. Sending native cryptocurrency is referred by the library as `Pay` as oppose to `Send` which is used for native assets. Inline meta-data is referred to as `Annotation` and non-inline meta-data is referred to as `Attachment`.

### Sending a pay transaction

**Code**:

```C#
    var txnCmd = _container.GetRequiredService<IMultiChainTransactionCommand>();
    var txid = new MultiChainFluent()
        .AddLogger(logger)
        .From(admin)
        .To(testUser1)
        .Pay(1_000_000)
        .CreateNormalTransaction(txnCmd)
            .AddSigner(signer)
            .Sign()
            .Send()
        ;
```

### Issue asset with non-inline meta-data

**Code**:

```C#
    var txid = new MultiChainFluent()
        .AddLogger(logger)
        .From(admin)
        .To(testUser1)
        .IssueAsset(1000)
        .With()
        .IssueDetails(assetName, 1, true)
        .AttachJson(new { Name = "Non-Inline meta-data" })
        .CreateNormalTransaction(txnCmd)
            .AddSigner(signer)
            .Sign()
            .Send()
        ;
```

### Send asset with inline meta-data

**Code**:

```C#
    var txid = new MultiChainFluent()
        .AddLogger(_logger)
        .From(_admin.NodeWallet)
        .To(_testUser1.NodeWallet)
        .IssueAsset(1000)
        .AnnotateJson(new { Name = "Inline meta-data" })
        .With()
        .IssueDetails(assetName, 1, true)
        .CreateNormalTransaction(_txnCmd)
            .AddSigner(new DefaultSigner(_admin.Ptekey))
            .Sign()
            .Send()
        ;
```

### Send asset using 2-of-3 multisig address

**Code**:

```C#
    var txid = new MultiChainFluent()
        .AddLogger(_logger)
        .From(multisig)
        .To(testuser1)
        .SendAsset(assetName, 1)
        .CreateMultiSigTransaction(txnCmd)
            .AddMultiSigSigner(signer1)
            .AddMultiSigSigner(signer2)
            .MultiSign(redeemScript)
            .Send()
        ;

```

### Sending asset using 2-of-3 multisig address in multi-stages

**Code**:

```C#

// Create the transaction hash for signing

    var request = new MultiChainFluent()
        .From(multisig)
        .To(testUser1)
        .SendAsset(assetName, 1)
        .CreateRawTransaction(txnCmd);

// Externally signed by user 1
    var signatures1 = new MultiChainFluent()
        .UseMultiSigTransaction(txnCmd)
        .AddMultiSigSigner(new DefaultSigner(_relayer1.Ptekey))
        .MultiSignPartial(request, state.RedeemScript);

// Externally signed by user 2
    var signatures2 = new MultiChainFluent()
        .UseMultiSigTransaction(txnCmd)
        .AddMultiSigSigner(new DefaultSigner(_relayer2.Ptekey))
        .MultiSignPartial(request,state.RedeemScript);

// Send signed transaction
    var txid = new MultiChainFluent()
        .UseMultiSigTransaction(txnCmd)
        .AddRawTransaction(request)
        .AddMultiSignatures(new List<string[]> { signatures1, signatures2 })
        .MultiSign(state.RedeemScript)
        .Send();

```

### Issue non-fungible asset

**Code**:

```C#
new MultiChainFluent()
    .From(_admin.NodeWallet)
    .To(_admin.NodeWallet)
        .IssueAsset(0)
    .With()
        .IssueNonFungibleAsset(nfaName)
    .CreateNormalTransaction(_txnCmd)
        .AddSigner(new DefaultSigner(_admin.Ptekey))
        .Sign()
        .Send()
    ;
```

### Issue non-fungible token

**Code**:

```C#
new MultiChainFluent()
    .From(_admin.NodeWallet)
    .To(_admin.NodeWallet)
        .IssueToken(nfaName,"nft1",1)
    .With()
    .CreateNormalTransaction(_txnCmd)
        .AddSigner(new DefaultSigner(_admin.Ptekey))
        .Sign()
        .Send()
        ;
```

### Send non-fungible token

**Code**:

```C#
new MultiChainFluent()
    .From(_admin.NodeWallet)
    .To(_testUser1.NodeWallet)
        .SendToken(nfaName, "nft1", 1)
    .With()
    .CreateNormalTransaction(_txnCmd)
        .AddSigner(new DefaultSigner(_admin.Ptekey))
        .Sign()
        .Send()
        ;
```

## Using Stream Query

The stream query syntax is used to make life easier when query stream items from multichain. The syntax will return the latest items in descending order by default.

**Code**:

```C#
IMultiChainStreamManager sm=container.GetRequiredService<IMultiChainStreamManager>();
var result = await sm.CreateStreamAsync(fromAddress, "testStream");
var query =await sm.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} WHERE key='foo' PAGE 0 SIZE 2", false);
Console.WriteLine(query.Result);
```

**Syntax:**

```
FROM <streamName> [WHERE (txid=<txid>|key=<key>|publish=<address>) [(DESC|ASC)] ] [PAGE page SIZE size]
```

**Example:**

1. Get last item from <streamName>

   `FROM <streamName>`

2. Get first item from <streamName>

   `FROM <streamName> ASC`

3. Get last 5 items from <streamName> in descending order, ie. if items are 1,2,3,4,5,6,7,8,9,10 will return 10,9,8,7,6

   `FROM <streamName> PAGE 0 SIZE 5`

4. Get first 5 items from <streamName> in ascending order, ie. if items are 1,2,3,4,5,6,7,8,9,10 will return 1,2,3,4,5

   `FROM <streamName> ASC PAGE 0 SIZE 5`

5. Get item by txid

   `FROM <streamName> WHERE txid='...'`

6. Get item by key

   `FROM <streamName> WHERE key='...'`

7. Get item by publisher wallet address

   `FROM <streamName> WHERE publisher='...'`

## Using the Web Socket Service

The MultiChainDotNet Web Socket Server is designed to broadcast transaction to web socket subscribers when the node's wallet is notified of a new transaction.

### Wallet Notification

The multichain.conf file on the seednode contains the runtime parameter `walletnotifynew=/root/notify.sh %j %n` for the daemon to send notification to the notify.sh script.

The notification will transmit the decoded raw transaction and blockheight to port `12018`

```sh
#!/bin/bash
echo $1
echo Block Height: $2
if [ -z $ReceivingHost ]; then
    ReceivingHost="http://localhost:12018/transaction"
fi
curl -s -X POST $ReceivingHost -H 'Content-Type:application/json' -d ''"$1"''
```

### Web Socket Server

The web socket server project is located in the MultiChainDotNet.Api.Service folder. It will run at port `12018` accepting POST request from the seednode at the transaction endpoint. Upon receiving the request, the web socket server will broadcast via the Publish endpoint to the web socket client.

### Web Socket Client using Javascript

```js
    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.min.js"></script>
    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:12018/transaction")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        if (connection.state !== 'Connected')
            await connection.start();

        connection.on("Publish", (raw) =>
        {
            // use the decoded raw transaction
            console.log(raw);
        });

    </script>
```

### Web Socket Client using C#

```cs
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:12018/transaction")
    .WithAutomaticReconnect()
    .Build();
...

connection.On<string>("Publish", (raw) =>
{
    // Use the decoded raw transaction
    Console.WriteLine(raw);
});
...

```

### Testing the API Listener

1. Go to the folder /test/MultiChainDotNet.SocketTest.
2. In the same folder, start a web server running at port:3000 or port:8080, eg. From 'visual Code', click on index.html and run `Live Preview` or `live-server` from node.
3. Open a browser at the server location.
4. Go to the same folder, open a terminal and run `dotnet run` to start a listener console.
5. Go to the same folder, open another terminal and run `./mc.sh` to send a test transaction to multichain. Alternatively, just run the command `send 12tDDPm72xRFqmQ96jJtqT4cCGwTHNVsz2A4HB 0` using multichain-cli

The result of the decoded raw transaction should be displayed on both the listener console and browser.

![demo](./img/socket-demo.gif)
