using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaExchange.ConsoleApp.Models;

namespace MetaExchange.ConsoleApp.Services
{
    public class ExecutionService : IExecutionService
    {
        public async Task<ExecutionPlan> GetBestExecutionPlanAsync(List<Exchange> exchanges, string orderType, double amount)
        {
            if (string.IsNullOrWhiteSpace(orderType))
                throw new ArgumentException("Order type must be specified.", nameof(orderType));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

            var executionPlan = new ExecutionPlan();

            if (orderType.Equals("Buy", StringComparison.OrdinalIgnoreCase))
            {
                // Collect all asks from all exchanges
                var allAsks = exchanges
                    .SelectMany(ex => ex.OrderBook.Asks)
                    .Select(ow => ow.Order)
                    .Where(o => o.Type.Equals("Sell", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(o => o.Price)
                    .ToList();

                double remainingAmount = amount;

                foreach (var ask in allAsks)
                {
                    if (remainingAmount <= 0)
                        break;

                    // Find the exchange containing this ask
                    var exchange = exchanges.FirstOrDefault(ex => ex.OrderBook.Asks.Any(a => a.Order.Id == ask.Id));

                    if (exchange == null)
                        continue;

                    // Check EUR balance
                    double maxAffordableAmount = exchange.AvailableFunds.Euro / ask.Price;
                    double purchasableAmount = Math.Min(ask.Amount, Math.Min(remainingAmount, maxAffordableAmount));

                    if (purchasableAmount <= 0)
                        continue;

                    // Add to execution plan
                    executionPlan.Orders.Add(new ExecutionOrder
                    {
                        ExchangeId = exchange.Id,
                        OrderId = ask.Id,
                        Type = "Buy",
                        Amount = purchasableAmount,
                        Price = ask.Price
                    });

                    // Update balances
                    exchange.AvailableFunds.Euro -= purchasableAmount * ask.Price;
                    exchange.AvailableFunds.Crypto += purchasableAmount;

                    remainingAmount -= purchasableAmount;
                }

                executionPlan.TotalCostOrRevenue = amount * allAsks.FirstOrDefault()?.Price ?? 0;
            }
            else if (orderType.Equals("Sell", StringComparison.OrdinalIgnoreCase))
            {
                // Collect all bids from all exchanges
                var allBids = exchanges
                    .SelectMany(ex => ex.OrderBook.Bids)
                    .Select(ow => ow.Order)
                    .Where(o => o.Type.Equals("Buy", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(o => o.Price)
                    .ToList();

                double remainingAmount = amount;

                foreach (var bid in allBids)
                {
                    if (remainingAmount <= 0)
                        break;

                    // Find the exchange containing this bid
                    var exchange = exchanges.FirstOrDefault(ex => ex.OrderBook.Bids.Any(b => b.Order.Id == bid.Id));

                    if (exchange == null)
                        continue;

                    // Check BTC balance
                    double availableBtc = exchange.AvailableFunds.Crypto;
                    double sellableAmount = Math.Min(bid.Amount, Math.Min(remainingAmount, availableBtc));

                    if (sellableAmount <= 0)
                        continue;

                    // Add to execution plan
                    executionPlan.Orders.Add(new ExecutionOrder
                    {
                        ExchangeId = exchange.Id,
                        OrderId = bid.Id,
                        Type = "Sell",
                        Amount = sellableAmount,
                        Price = bid.Price
                    });

                    // Update balances
                    exchange.AvailableFunds.Euro += sellableAmount * bid.Price;
                    exchange.AvailableFunds.Crypto -= sellableAmount;

                    remainingAmount -= sellableAmount;
                }

                executionPlan.TotalCostOrRevenue = amount * allBids.FirstOrDefault()?.Price ?? 0;
            }
            else
            {
                throw new ArgumentException("Invalid order type. Must be 'Buy' or 'Sell'.", nameof(orderType));
            }

            // Optionally, you can calculate the total cost or revenue more accurately
            if (orderType.Equals("Buy", StringComparison.OrdinalIgnoreCase))
            {
                executionPlan.TotalCostOrRevenue = executionPlan.Orders.Sum(o => o.Amount * o.Price);
            }
            else if (orderType.Equals("Sell", StringComparison.OrdinalIgnoreCase))
            {
                executionPlan.TotalCostOrRevenue = executionPlan.Orders.Sum(o => o.Amount * o.Price);
            }

            return executionPlan;
        }
    }
}
