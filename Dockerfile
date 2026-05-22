# Estágio 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/TodoApi/*.csproj ./src/TodoApi/
RUN dotnet restore src/TodoApi/TodoApi.csproj

COPY . .
RUN dotnet publish src/TodoApi/TodoApi.csproj -c Release -o /publish

# Estágio 2: runtime (imagem menor)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "TodoApi.dll"]
