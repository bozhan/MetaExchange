# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining source code and build the project
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out ./

# Set the environment variable for the ExchangesDirectory
ENV ExchangesDirectory=/app/Exchanges

# Create the Exchanges directory and add sample data
RUN mkdir /app/Exchanges
# Optionally, copy sample JSON files if available
COPY Exchanges/*.json /app/Exchanges/

# Expose port 80
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "MetaExchange.WebApi.dll"]
