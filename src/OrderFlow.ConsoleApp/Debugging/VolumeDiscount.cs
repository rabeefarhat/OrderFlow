// src/OrderFlow.ConsoleApp/Debugging/VolumeDiscount.cs
using OrderFlow.ConsoleApp.Models;

namespace OrderFlow.ConsoleApp.Debugging;

// Debugging exercise from L01 M6. The bug was a loop starting at index 1,
// which skipped the first line; summing every item removes the index entirely.
public static class VolumeDiscount
{
    public const int MinimumQuantity = 10;
    public const decimal Rate = 0.05m;

    public static decimal CalculateDiscountedSubtotal(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.Items.Sum(CalculateLineTotal);
    }

    private static decimal CalculateLineTotal(OrderItem item)
    {
        decimal discount = item.Quantity >= MinimumQuantity
            ? Math.Round(item.Subtotal * Rate, 2, MidpointRounding.AwayFromZero)
            : 0m;

        return item.Subtotal - discount;
    }
}