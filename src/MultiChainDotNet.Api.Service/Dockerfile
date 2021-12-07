#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 12028

ENV MULTICHAINDOTNET_MultiChainConfiguration__Node__NetworkAddress=host.docker.internal
ENV MULTICHAINDOTNET_MultiChainConfiguration__Node__NetworkPort=12021
ENV MULTICHAINDOTNET_MultiChainConfiguration__Node__ChainName=chain1
ENV MULTICHAINDOTNET_MultiChainConfiguration__Node__RpcUserName=multichainrpc
ENV MULTICHAINDOTNET_MultiChainConfiguration__Node__RpcPassword=C6n97oxTJrEqmwvVrWGP5TgHpyewRjz2x3soDQKLFkWq

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/MultiChainDotNet.Api.Service/MultiChainDotNet.Api.Service.csproj", "src/MultiChainDotNet.Api.Service/"]
RUN dotnet restore "src/MultiChainDotNet.Api.Service/MultiChainDotNet.Api.Service.csproj"
COPY . .
WORKDIR "/src/src/MultiChainDotNet.Api.Service"
RUN dotnet build "MultiChainDotNet.Api.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiChainDotNet.Api.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiChainDotNet.Api.Service.dll"]