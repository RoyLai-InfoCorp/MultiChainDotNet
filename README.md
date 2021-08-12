# MultiChainDotNet

## Features

* The build directory contains docker-compose script used to launch a 4-node multichain network on the development machine running WSL(Ubuntu).
* The Core library contains the wrapper of MultiChain RPC API and included relevant strongly typed class for request and response.
* The Fluent library is a fluent framework for constructing multichain raw transactions and signing transactions externally.
* The Managers library is a high-level abstraction of the underlying classes designed for dependency injection.
* The libraries incorporated some enhancements using HttpClientFactory and Polly to manage the resiliency of calls.
* Uses sql-style command for stream query.

## Getting Started

### 1. Install Docker for Windows Desktop with WSL2

https://docs.docker.com/docker-for-windows/wsl/

### 2. Clone this repo

https://github.com/RoyLai-InfoCorp/MultiChainDotNet

### 3. Startup 1 seednode and 3 test nodes

Go to /build/multichain directory

```bash
sh$ ./multichain/create-base-image.sh
sh$ docker-compose up
```

The output should show `Node ready.` for all 4 nodes.
If nodes doesn't start up, press CTRL-C, enter `docker-compose down` and try again.

### 4. Test the connection

Each node should be connected to 3 other nodes.

```bash
sh$ docker exec -it multichain-node1 multichain-cli sennet getpeerinfo
sh$ docker exec -it multichain-node2 multichain-cli sennet getpeerinfo
sh$ docker exec -it multichain-node3 multichain-cli sennet getpeerinfo
```

### 5. Compile and Test

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

   ```FROM <streamName>```

2. Get first item from <streamName>

   ```FROM <streamName> ASC```

3. Get last 5 items from <streamName> in descending order, ie. if items are 1,2,3,4,5,6,7,8,9,10 will return 10,9,8,7,6

   ```FROM <streamName> PAGE 0 SIZE 5```

4. Get first 5 items from <streamName> in ascending order, ie. if items are 1,2,3,4,5,6,7,8,9,10 will return 1,2,3,4,5

   ```FROM <streamName> ASC PAGE 0 SIZE 5```

5. Get item by txid

   ```FROM <streamName> WHERE txid='...'```

6. Get item by key

   ```FROM <streamName> WHERE key='...'```

7. Get item by publisher wallet address

   ```FROM <streamName> WHERE publisher='...'```
