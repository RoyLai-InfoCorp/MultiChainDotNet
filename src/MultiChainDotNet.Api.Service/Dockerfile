#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 12028

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