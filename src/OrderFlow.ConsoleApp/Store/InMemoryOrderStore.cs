// src/OrderFlow.ConsoleApp/Store/InMemoryOrderStore.cs
using OrderFlow.ConsoleApp.Models;

namespace OrderFlow.ConsoleApp.Store;

// Holds orders in memory for this run of the program. Every method returns a
// finished result ([.. ]), never a live query, so callers can't see later changes.
public sealed class InMemoryOrderStore
{
    private readonly List<Order> _orders = [];                                  // ①

    public int Count => _orders.Count;

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);                               // ②
        _orders.Add(order);
    }

    public IReadOnlyList<Order> FindByCustomer(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            return [];                                                          // ③
        }

        string wanted = customerName.Trim();
        return
        [
            .. _orders                                                          // ④
                .Where(order => IsCustomer(order, wanted))                      // ⑤
                .OrderByDescending(order => order.CalculateTotal()),            // ⑥
        ];
    }

    public Order? FindHighestValueOrder(string customerName)                    // ⑦
    {
        IReadOnlyList<Order> orders = FindByCustomer(customerName);
        if (orders.Count == 0)
        {
            return null;                                                        // ⑧
        }

        decimal highest = orders.Max(order => order.CalculateTotal());          // ⑨
        return orders.First(order => order.CalculateTotal() == highest);        // ⑩
    }

    public bool HasOrdersFor(string customerName) =>
        !string.IsNullOrWhiteSpace(customerName)
        && _orders.Any(order => IsCustomer(order, customerName.Trim()));        // ⑪

    public IReadOnlyList<Order> FindByStatus(OrderStatus status) =>
        [.. _orders.Where(order => order.Status == status)];

    public IReadOnlyList<Order> AllByTotalDescending() =>
        [.. _orders.OrderByDescending(order => order.CalculateTotal())];

    public IReadOnlyList<CustomerSummary> SummariseByCustomer() =>
    [
        .. _orders
            .GroupBy(order => order.CustomerName, StringComparer.OrdinalIgnoreCase)   // ⑫
            .Select(group => new CustomerSummary(                                     // ⑬
                group.Key,
                group.Count(),
                group.Sum(order => order.CalculateTotal())))
            .OrderByDescending(summary => summary.TotalSpent),
    ];

    public IReadOnlyDictionary<OrderStatus, int> CountByStatus()
    {
        Dictionary<OrderStatus, int> counts = new();                            // ⑭
        foreach (OrderStatus status in Enum.GetValues<OrderStatus>())           // ⑮
        {
            counts[status] = 0;
        }

        foreach (Order order in _orders)
        {
            counts[order.Status]++;                                             // ⑯
        }

        return counts;
    }

    private static bool IsCustomer(Order order, string customerName) =>
        string.Equals(order.CustomerName, customerName, StringComparison.OrdinalIgnoreCase);  // ⑰
}