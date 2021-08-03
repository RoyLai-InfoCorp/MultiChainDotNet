#!/bin/bash

echo '
FROM ubuntu AS base
FROM base AS build
SHELL ["/bin/bash","-c"]
RUN apt update
RUN apt install wget -y
WORKDIR /tmp
RUN wget https://www.multichain.com/download/multichain-2.1.2.tar.gz 
RUN tar -xvzf multichain-2.1.2.tar.gz
WORKDIR /tmp/multichain-2.1.2
FROM base AS final
COPY --from=build /tmp/multichain-2.1.2 /usr/local/bin
' | docker build -t multichain-base:2.1.2 -