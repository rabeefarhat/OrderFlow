// src/OrderFlow.ConsoleApp/Models/Order.cs
namespace OrderFlow.ConsoleApp.Models;

public sealed class Order
{
    public const int MaxCustomerNameLength = 80;
    public const decimal VatRate = 0.21m;                                       // ①

    private readonly List<OrderItem> _items = new();                            // ②

    public Order(string customerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);                // ③
        string trimmedName = customerName.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            trimmedName.Length, MaxCustomerNameLength, nameof(customerName));

        CustomerName = trimmedName;
    }

    public string CustomerName { get; }

    public OrderStatus Status { get; private set; } = OrderStatus.Draft;        // ④

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();               // ⑤

    public int TotalQuantity
    {
        get
        {
            int total = 0;
            foreach (OrderItem item in _items)
            {
                total += item.Quantity;
            }

            return total;
        }
    }

    public void AddItem(Product product, int quantity)                          // ⑥
    {
        EnsureDraft("add an item");
        _items.Add(new OrderItem(product, quantity));                           // ⑦
    }

    public void Submit()                                                        // ⑧
    {
        EnsureDraft("submit it");
        if (_items.Count == 0)
        {
            throw new InvalidOperationException(
                "An order needs at least one item before it can be submitted.");
        }

        Status = OrderStatus.Submitted;
    }

    public decimal CalculateSubtotal()
    {
        decimal subtotal = 0m;
        foreach (OrderItem item in _items)
        {
            subtotal += item.Subtotal;
        }

        return subtotal;
    }

    public decimal CalculateVat() =>                                            // ⑨
        Math.Round(CalculateSubtotal() * VatRate, 2, MidpointRounding.AwayFromZero);

    public decimal CalculateTotal() => CalculateSubtotal() + CalculateVat();

    private void EnsureDraft(string action)                                     // ⑩
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException(
                $"You can only {action} while the order is a draft; it is {Status}.");
        }
    }
}