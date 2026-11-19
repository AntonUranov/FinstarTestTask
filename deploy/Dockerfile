# syntax=docker/dockerfile:1
ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG PROJECT
WORKDIR /repo
COPY src ./src
RUN dotnet restore "src/${PROJECT}/${PROJECT}.csproj"
RUN dotnet publish "src/${PROJECT}/${PROJECT}.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
ARG PROJECT
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV APP_DLL=${PROJECT}.dll
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet $APP_DLL"]
