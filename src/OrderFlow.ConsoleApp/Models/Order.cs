// src/OrderFlow.ConsoleApp/Models/Order.cs
namespace OrderFlow.ConsoleApp.Models;

public sealed class Order
{
    public const int MaxCustomerNameLength = 80;
    public const decimal VatRate = 0.21m;

    private readonly List<OrderItem> _items = [];                               // ①

    public Order(string customerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);
        string trimmedName = customerName.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            trimmedName.Length, MaxCustomerNameLength, nameof(customerName));

        CustomerName = trimmedName;
    }

    public string CustomerName { get; }

    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public int TotalQuantity => _items.Sum(item => item.Quantity);              // ②

    public void AddItem(Product product, int quantity)
    {
        EnsureDraft("add an item");
        _items.Add(new OrderItem(product, quantity));
    }

    public void Submit()
    {
        EnsureDraft("submit it");
        if (_items.Count == 0)
        {
            throw new InvalidOperationException(
                "An order needs at least one item before it can be submitted.");
        }

        Status = OrderStatus.Submitted;
    }

    public decimal CalculateSubtotal() => _items.Sum(item => item.Subtotal);    // ③

    public decimal CalculateVat() =>
        Math.Round(CalculateSubtotal() * VatRate, 2, MidpointRounding.AwayFromZero);

    public decimal CalculateTotal() => CalculateSubtotal() + CalculateVat();

    private void EnsureDraft(string action)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException(
                $"You can only {action} while the order is a draft; it is {Status}.");
        }
    }
}