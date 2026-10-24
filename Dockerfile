# syntax=docker/dockerfile:1

# Um Dockerfile, dois alvos: API e worker compartilham Domain, Application e
# Infrastructure, e o estágio de build comum compila essas camadas uma vez.
#
#   docker build --target api    -t remedin-api .
#   docker build --target worker -t remedin-worker .

ARG DOTNET_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Só os projetos primeiro. Mudar código não invalida a camada de pacotes, e o
# restore — a parte lenta — fica em cache entre builds.
COPY src/Remedin.Domain/Remedin.Domain.csproj                 src/Remedin.Domain/
COPY src/Remedin.Application/Remedin.Application.csproj       src/Remedin.Application/
COPY src/Remedin.Infrastructure/Remedin.Infrastructure.csproj src/Remedin.Infrastructure/
COPY src/Remedin.Api/Remedin.Api.csproj                       src/Remedin.Api/
COPY src/Remedin.Worker/Remedin.Worker.csproj                 src/Remedin.Worker/

RUN dotnet restore src/Remedin.Api/Remedin.Api.csproj \
 && dotnet restore src/Remedin.Worker/Remedin.Worker.csproj

COPY src/ src/

FROM build AS publish-api
RUN dotnet publish src/Remedin.Api/Remedin.Api.csproj \
    --configuration Release --no-restore --output /app

FROM build AS publish-worker
RUN dotnet publish src/Remedin.Worker/Remedin.Worker.csproj \
    --configuration Release --no-restore --output /app

# Imagens chiseled: sem shell, sem gerenciador de pacotes, usuário sem
# privilégio por padrão. A variante -extra é obrigatória aqui, e não escolha:
# a leitura dos números da CMED, da dosagem e a ordenação dos estados usam a
# cultura pt-BR, e a chiseled comum roda sem ICU, onde pedir pt-BR lança
# exceção.

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble-chiseled-extra AS api
WORKDIR /app
COPY --from=publish-api /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Remedin.Api.dll"]

# O worker não serve HTTP, então a imagem de runtime basta. Ele roda a carga e
# termina: o código de saída é o que diz ao agendador se deu certo.
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION}-noble-chiseled-extra AS worker
WORKDIR /app
COPY --from=publish-worker /app .
ENTRYPOINT ["dotnet", "Remedin.Worker.dll"]
