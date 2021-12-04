#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/MultiChainDotNet.Listener.Service/MultiChainDotNet.Listener.Service.csproj", "src/MultiChainDotNet.Listener.Service/"]
RUN dotnet restore "src/MultiChainDotNet.Listener.Service/MultiChainDotNet.Listener.Service.csproj"
COPY . .
WORKDIR "/src/src/MultiChainDotNet.Listener.Service"
RUN dotnet build "MultiChainDotNet.Listener.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiChainDotNet.Listener.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiChainDotNet.Listener.Service.dll"]