FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/AtlanticCity.Gateway/AtlanticCity.Gateway.csproj", "src/Backend/AtlanticCity.Gateway/"]
COPY ["src/Backend/AtlanticCity.Shared/AtlanticCity.Shared.csproj", "src/Backend/AtlanticCity.Shared/"]
RUN dotnet restore "src/Backend/AtlanticCity.Gateway/AtlanticCity.Gateway.csproj"
COPY . .
WORKDIR "/src/src/Backend/AtlanticCity.Gateway"
RUN dotnet build "AtlanticCity.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AtlanticCity.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AtlanticCity.Gateway.dll"]
