FROM mcr.microsoft.com/dotnet/sdk:11.0-alpine AS build
RUN apk add --no-cache clang build-base zlib-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dropper/Dropper.csproj", "Dropper/"]
RUN dotnet restore "Dropper/Dropper.csproj" -r linux-musl-x64
COPY . .
RUN dotnet publish "Dropper/Dropper.csproj" \
    -c $BUILD_CONFIGURATION \
    -r linux-musl-x64 \
    --self-contained \
    --no-restore \
    -o /app/publish \
    /p:StripSymbols=true

FROM mcr.microsoft.com/dotnet/runtime-deps:11.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["/app/Dropper"]
