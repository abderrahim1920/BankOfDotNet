FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["BankOfDotNet.API/BankOfDotNet.API.csproj", "BankOfDotNet.API/"]
RUN dotnet restore "BankOfDotNet.API/BankOfDotNet.API.csproj"
COPY . .
WORKDIR "/src/BankOfDotNet.API"
RUN dotnet build "BankOfDotNet.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BankOfDotNet.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BankOfDotNet.API.dll"]
