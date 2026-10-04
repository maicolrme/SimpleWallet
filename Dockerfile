FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SimpleWallet.csproj .
RUN dotnet restore SimpleWallet.csproj

COPY . .
RUN dotnet publish SimpleWallet.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/wallet.db"
ENV DataProtection__KeysPath="/app/data/keys"

EXPOSE 8080

ENTRYPOINT ["dotnet", "SimpleWallet.dll"]
