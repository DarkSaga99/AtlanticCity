FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/AtlanticCity.Services.Processing/AtlanticCity.Services.Processing.csproj", "src/Backend/AtlanticCity.Services.Processing/"]
COPY ["src/Backend/AtlanticCity.Shared/AtlanticCity.Shared.csproj", "src/Backend/AtlanticCity.Shared/"]
RUN dotnet restore "src/Backend/AtlanticCity.Services.Processing/AtlanticCity.Services.Processing.csproj"
COPY . .
WORKDIR "/src/src/Backend/AtlanticCity.Services.Processing"
RUN dotnet build "AtlanticCity.Services.Processing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AtlanticCity.Services.Processing.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AtlanticCity.Services.Processing.dll"]
