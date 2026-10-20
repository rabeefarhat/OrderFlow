// src/OrderFlow.ConsoleApp/Models/OrderItem.cs
namespace OrderFlow.ConsoleApp.Models;

// 🔤 [C# 12] primary constructor. The parameters are only used to initialise
// properties below, never inside methods, so nothing mutable is captured.
public sealed class OrderItem(Product product, int quantity)                   // ①
{
    public const int MinQuantity = 1;                                          // ②
    public const int MaxQuantity = 10_000;

    public Product Product { get; } =
        product ?? throw new ArgumentNullException(nameof(product));          // ③

    public string ProductName { get; } = product.Name;                         // ④

    public decimal UnitPrice { get; } = product.UnitPrice;                     // ⑤

    public int Quantity { get; } = EnsureValidQuantity(quantity);              // ⑥

    public decimal Subtotal => UnitPrice * Quantity;                           // ⑦

    private static int EnsureValidQuantity(int value)                          // ⑧
    {
        if (value is < MinQuantity or > MaxQuantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Quantity),
                $"A quantity must be between {MinQuantity} and {MaxQuantity}.");
        }

        return value;
    }
}