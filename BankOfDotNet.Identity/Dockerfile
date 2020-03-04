FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["BankOfDotNet.Identity/BankOfDotNet.Identity.csproj", "BankOfDotNet.Identity/"]
RUN dotnet restore "BankOfDotNet.Identity/BankOfDotNet.Identity.csproj"
COPY . .
WORKDIR "/src/BankOfDotNet.Identity"
RUN dotnet build "BankOfDotNet.Identity.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BankOfDotNet.Identity.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BankOfDotNet.Identity.dll"]
