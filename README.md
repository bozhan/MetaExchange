# MetaExchange Console Application

## Overview

MetaExchange is a .NET Core console application that aggregates order books from multiple cryptocurrency exchanges to provide the best possible buy or sell prices for a specified amount of Bitcoin (BTC).

## Features

- **Load Multiple Exchanges:** Reads JSON files representing different exchanges.
- **Best Execution Plan:** Computes the optimal set of orders to execute based on user input.
- **Constraints Handling:** Respects EUR and BTC balances without transferring funds between exchanges.
- **Logging:** Provides informative logs for monitoring and debugging.
- **Unit Testing:** Ensures reliability through comprehensive unit tests.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 7.0)
- Git

### Setup

1. **Clone the Repository:**

   ```bash
   git clone <url>
   cd MetaExchange
