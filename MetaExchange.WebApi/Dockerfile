# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the WebApi and ConsoleApp project files
COPY MetaExchange.WebApi/MetaExchange.WebApi.csproj MetaExchange.WebApi/
COPY MetaExchange.ConsoleApp/MetaExchange.ConsoleApp.csproj MetaExchange.ConsoleApp/

# Restore dependencies
RUN dotnet restore MetaExchange.WebApi/MetaExchange.WebApi.csproj

# Copy the source code
COPY MetaExchange.WebApi/ MetaExchange.WebApi/
COPY MetaExchange.ConsoleApp/ MetaExchange.ConsoleApp/

# Publish the WebApi project
WORKDIR /src/MetaExchange.WebApi
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published output
COPY --from=build /app/publish ./

# Set environment variable for ExchangesDirectory
ENV ExchangesDirectory=/app/Exchanges

# Create the Exchanges directory
RUN mkdir /app/Exchanges

# (Optional) Copy sample exchange JSON files
COPY MetaExchange.WebApi/Exchanges/*.json /app/Exchanges/

# Expose port 80
EXPOSE 80

# Define the entry point
ENTRYPOINT ["dotnet", "MetaExchange.WebApi.dll"]
