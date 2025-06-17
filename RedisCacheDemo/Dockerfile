FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5129

ENV ASPNETCORE_URLS=http://+:5129

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["RedisCacheDemo/RedisCacheDemo.csproj", "RedisCacheDemo/"]
RUN dotnet restore "RedisCacheDemo/RedisCacheDemo.csproj"
COPY . .
WORKDIR "/src/RedisCacheDemo"
RUN dotnet build "RedisCacheDemo.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "RedisCacheDemo.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedisCacheDemo.dll"]
