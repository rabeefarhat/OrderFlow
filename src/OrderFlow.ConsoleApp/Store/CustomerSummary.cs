// src/OrderFlow.ConsoleApp/Store/CustomerSummary.cs
namespace OrderFlow.ConsoleApp.Store;

// Only what the report screen needs: no order lines, no products.
public sealed record CustomerSummary(string CustomerName, int OrderCount, decimal TotalSpent);