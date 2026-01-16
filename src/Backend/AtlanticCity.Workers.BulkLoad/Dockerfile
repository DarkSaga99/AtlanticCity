FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/AtlanticCity.Workers.BulkLoad/AtlanticCity.Workers.BulkLoad.csproj", "src/Backend/AtlanticCity.Workers.BulkLoad/"]
COPY ["src/Backend/AtlanticCity.Shared/AtlanticCity.Shared.csproj", "src/Backend/AtlanticCity.Shared/"]
RUN dotnet restore "src/Backend/AtlanticCity.Workers.BulkLoad/AtlanticCity.Workers.BulkLoad.csproj"
COPY . .
WORKDIR "/src/src/Backend/AtlanticCity.Workers.BulkLoad"
RUN dotnet build "AtlanticCity.Workers.BulkLoad.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AtlanticCity.Workers.BulkLoad.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AtlanticCity.Workers.BulkLoad.dll"]
