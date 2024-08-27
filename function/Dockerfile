FROM dxpprivate.azurecr.io/ahi-build:6.0 AS installer-env
WORKDIR .
COPY function/. ./audit/function/
COPY NuGet.Config ./
# COPY framework/. ./framework/

RUN cd ./audit/function && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj -c release -o /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:3.0-appservice
FROM dxpprivate.azurecr.io/ahi-function:6.0
COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]