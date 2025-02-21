#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV DOTNET_RUNNING_IN_CONTAINER=true 

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LlmFrontend/LlmFrontend.csproj", "LlmFrontend/"]
COPY ["LlmCommon/LlmCommon.csproj", "LlmCommon/"]
RUN dotnet restore "/src/LlmFrontend/LlmFrontend.csproj"
COPY . .
WORKDIR "/src/LlmFrontend"
RUN dotnet build "./LlmFrontend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LlmFrontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "LlmFrontend.dll"]
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY ["LlmFrontend/nginx.conf", "/etc/nginx/nginx.conf"]


#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /src
#COPY ["LlmFrontend/LlmFrontend.csproj", "LlmFrontend/"]
#COPY ["Cjm.Common/Cjm.Common.csproj", "Cjm.Common/"]
#RUN dotnet restore "./LlmFrontend/./LlmFrontend.csproj"
#COPY . .
#WORKDIR "/src/LlmFrontend"
#RUN dotnet build "./LlmFrontend.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "./LlmFrontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#

