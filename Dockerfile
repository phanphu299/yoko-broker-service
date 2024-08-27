# SDK build image
FROM dxpprivate.azurecr.io/ahi-build:6.0-alpine3.19 AS build
WORKDIR .
COPY NuGet.Config ./
COPY src/Broker.Api/*.csproj         ./src/Broker.Api/
COPY src/Broker.Application/*.csproj ./src/Broker.Application/
COPY src/Broker.Domain/*.csproj      ./src/Broker.Domain/
COPY src/Broker.Persistence/*.csproj ./src/Broker.Persistence/
RUN dotnet restore ./src/Broker.Api/*.csproj /property:Configuration=Release -nowarn:msb3202,nu1503

COPY src/ ./src
RUN dotnet publish -r linux-x64 ./src/Broker.Api/*.csproj --no-self-contained --no-restore -c Release -o /app/out

# Run time image
FROM dxpprivate.azurecr.io/ahi-runtime:6.0-alpine3.19 as final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Broker.Api.dll"]
