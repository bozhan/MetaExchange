# MetaExchange Web API

## Overview

MetaExchange Web API is a .NET Core-based service that aggregates order books from multiple cryptocurrency exchanges to provide optimal buy or sell execution plans for Bitcoin (BTC) transactions.

## Features

- **Multiple Exchange Support:** Integrates with various cryptocurrency exchanges using JSON-based order books.
- **Optimal Execution Plans:** Calculates the best set of orders to maximize efficiency based on user input.
- **Balance Constraints:** Ensures EUR and BTC balances are respected without inter-exchange transfers.
- **Logging:** Comprehensive logging for monitoring and debugging.
- **Unit Testing:** Robust tests to ensure reliability and correctness.
- **Docker Deployment:** Easily deployable using Docker containers.

## Getting Started

### Prerequisites

- [.NET SDK 7.0](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)
- Git

### Setup

1. **Clone the Repository:**

   ```bash
   git clone <repository-url>
   cd MetaExchange
   ```

2. **Restore Dependencies:**

   ```bash
   dotnet restore
   ```

3. **Build the Project:**

   ```bash
   dotnet build
   ```

### Running Locally

1. **Navigate to the Web API Project:**

   ```bash
   cd MetaExchange.WebApi
   ```

2. **Run the Application:**

   ```bash
   dotnet run
   ```

   The API will be available at `http://localhost:5000`.

### Running with Docker

1. **Build the Docker Image:**

   ```bash
   docker build -t metaexchange-webapi:latest .
   ```

2. **Run the Docker Container:**

   ```bash
   docker run -d -p 8080:80 --name metaexchange-api metaexchange-webapi:latest
   ```

   The API will be accessible at `http://localhost:8080`.

### Testing

1. **Navigate to the Tests Project:**

   ```bash
   cd MetaExchange.WebApi.Tests
   ```

2. **Run Tests:**

   ```bash
   dotnet test
   ```

## Usage

- **Get Execution Plan:**

  ```http
  GET /api/execution/plan?orderType=Buy&amount=7
  ```

  **Parameters:**
  
  - `orderType`: "Buy" or "Sell"
  - `amount`: Amount of BTC to transact

## License

This project is licensed under the MIT License.