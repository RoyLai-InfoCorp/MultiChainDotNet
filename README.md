# MultiChainDotNet

## Setup the blockchain network

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

## 4. Test the connection

Each node should be connected to 3 other nodes.

```bash
sh$ docker exec -it multichain-node1 multichain-cli senc2 getpeerinfo
sh$ docker exec -it multichain-node2 multichain-cli senc2 getpeerinfo
sh$ docker exec -it multichain-node3 multichain-cli senc2 getpeerinfo
```

## Compile and Test

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


