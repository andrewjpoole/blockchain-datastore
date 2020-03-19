FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["DataStore/DataStore.csproj", "DataStore/"]
COPY ["DataStore.Contracts/DataStore.Contracts.csproj", "DataStore.Contracts/"]
COPY ["DataStore.BlockChain/DataStore.Blockchain.csproj", "DataStore.BlockChain/"]
RUN dotnet restore "DataStore/DataStore.csproj"
COPY . .
WORKDIR "/src/DataStore"
RUN dotnet build "DataStore.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DataStore.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DataStore.dll"]
