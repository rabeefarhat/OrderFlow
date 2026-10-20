// src/OrderFlow.ConsoleApp/Models/Product.cs
namespace OrderFlow.ConsoleApp.Models;

public sealed class Product                                                   // ①
{
    public const int MinNameLength = 2;                                       // ②
    public const int MaxNameLength = 40;
    public const decimal MaxUnitPrice = 100_000m;

    public Product(string name, decimal unitPrice, int stockQuantity)         // ③
    {
        ArgumentNullException.ThrowIfNull(name);                              // ④
        string trimmedName = name.Trim();
        if (trimmedName.Length is < MinNameLength or > MaxNameLength)         // ⑤
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                $"A product name needs {MinNameLength} to {MaxNameLength} characters.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(stockQuantity);           // ⑥

        Name = trimmedName;
        UnitPrice = unitPrice;                                                // ⑦
        StockQuantity = stockQuantity;
    }

    public string Name { get; }                                               // ⑧

    // 🔤 [C# 14] field keyword: a validating setter with no hand-written backing field.
    public decimal UnitPrice                                                  // ⑨
    {
        get;
        set
        {
            if (value < 0m || value > MaxUnitPrice || value != decimal.Round(value, 2))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"A unit price must be between 0 and {MaxUnitPrice} with whole cents.");
            }

            field = value;                                                    // ⑩
        }
    }

    public int StockQuantity { get; }                                         // ⑪
}