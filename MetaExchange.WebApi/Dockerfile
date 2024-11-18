# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY MetaExchange.sln .
COPY MetaExchange.WebApi/MetaExchange.WebApi.csproj MetaExchange.WebApi/
COPY MetaExchange.ConsoleApp/MetaExchange.ConsoleApp.csproj MetaExchange.ConsoleApp/
RUN dotnet restore

# Copy all source code and build the project
COPY . .
WORKDIR /src/MetaExchange.WebApi
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set environment variables if needed
ENV ExchangesDirectory=/app/Exchanges

# Create the Exchanges directory
RUN mkdir /app/Exchanges

# (Optional) Copy sample exchange JSON files
COPY MetaExchange.WebApi/Exchanges/*.json /app/Exchanges/

# Expose port 80
EXPOSE 80

# Define the entry point
ENTRYPOINT ["dotnet", "MetaExchange.WebApi.dll"]
