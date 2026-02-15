# Stage 1: Build .NET API (and generate OpenAPI spec)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-api
WORKDIR /app

# Needed for the MSBuild GenerateOpenApi target (dotnet swagger)
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install --global swashbuckle.aspnetcore.cli --version 10.1.0
RUN ln -sf /root/.dotnet/tools/swagger /root/.dotnet/tools/dotnet-swagger

COPY VerticalSlice.Web.Api/*.csproj ./
RUN dotnet restore
COPY VerticalSlice.Web.Api/ .
RUN dotnet publish -c Release -o out

# Stage 2: Build React Application (prebuild runs Orval)
FROM node:20-alpine AS build-react
WORKDIR /app/client
RUN npm install -g npm@11

COPY clientapp/package*.json ./
RUN npm ci

COPY clientapp/ .

# Make OpenAPI available at the path expected by orval.config.ts (../VerticalSlice.Web.Api/openapi.json)
RUN mkdir -p /app/VerticalSlice.Web.Api
COPY --from=build-api /app/openapi.json /app/VerticalSlice.Web.Api/openapi.json

RUN npm run build -- --mode production

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-react /app/client/build ./wwwroot
COPY --from=build-api /app/out .
ENTRYPOINT ["dotnet", "VerticalSlice.Web.Api.dll"]
