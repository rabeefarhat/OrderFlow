// src/OrderFlow.ConsoleApp/Program.cs
// OrderFlow — P01.3: approval rules, a looping menu and reusable methods.

using System.Globalization;

// ── Settings: every business number has a name ──────────────────
const string CultureName = "nl-BE";                                  // ①
const decimal VatRate = 0.21m;
const decimal TeamLeadThreshold = 250m;
const decimal ManagerThreshold = 1_000m;
const int BulkQuantityThreshold = 50;
const decimal MaxUnitPrice = 100_000m;
const int MaxQuantity = 10_000;
const int MaxProductNameLength = 40;

// ── The program itself ──────────────────────────────────────────
PrintWelcome();                                                      // ②
RunMenu();
Console.WriteLine("Goodbye from OrderFlow.");

// ── Menu ────────────────────────────────────────────────────────
static void RunMenu()
{
    while (true)                                                     // ③
    {
        PrintMenu();
        string? line = Console.ReadLine();
        if (line is null)                                            // ④
        {
            return;
        }

        switch (line.Trim())                                         // ⑤
        {
            case "1":
                PriceNewOrderLine();
                break;
            case "2":
                ShowSampleOrders();
                break;
            case "3":
                ShowQuantityTable();
                break;
            case "4":
                ShowNumericTraps();
                break;
            case "0":
                return;                                              // ⑥
            default:
                Console.WriteLine($"  Rejected: '{line.Trim()}' is not a menu option. Type one of the numbers above.");
                break;
        }

        Console.WriteLine();
    }
}

static void PrintMenu()
{
    string[] options =                                               // ⑦
    {
        "1  Price a new order line",
        "2  Show the sample orders",
        "3  Show a quantity table (approval boundaries)",
        "4  Show the numeric traps (decimal vs double)",
        "0  Exit",
    };

    Console.WriteLine("Menu");
    foreach (string option in options)                               // ⑧
    {
        Console.WriteLine($"  {option}");
    }

    Console.Write("Your choice: ");
}

// ── Use case: price one order line typed by the user ────────────
static void PriceNewOrderLine()
{
    Console.WriteLine("Press Enter on an empty line at any question to cancel.");

    if (ReadProductName() is not string productName)                 // ⑨
    {
        PrintCancelled();
        return;                                                      // ⑩
    }

    if (ReadUnitPrice() is not decimal unitPrice)
    {
        PrintCancelled();
        return;
    }

    if (ReadQuantity() is not int quantity)
    {
        PrintCancelled();
        return;
    }

    PrintOrderLine(productName, unitPrice, quantity);                // ⑪
}

static void PrintCancelled() => Console.WriteLine("Cancelled - nothing was priced.");

// ── Input: read, convert, validate, ask again ───────────────────
static string? ReadProductName()
{
    while (true)
    {
        Console.Write("Product name: ");
        string? input = Console.ReadLine()?.Trim();                  // ⑫
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        string? error = ValidateProductName(input);
        if (error is null)
        {
            return input;
        }

        Console.WriteLine($"  Rejected: {error}");
    }
}

static decimal? ReadUnitPrice()
{
    CultureInfo belgian = CultureInfo.GetCultureInfo(CultureName);
    NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;    // ⑬

    while (true)
    {
        Console.Write("Unit price in euro, with a comma (for example 9,99): ");
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        if (!decimal.TryParse(input, style, belgian, out decimal price))                    // ⑭
        {
            Console.WriteLine($"  Rejected: '{input}' is not a price. Use digits and one comma, like 9,99.");
            continue;                                                // ⑮
        }

        string? error = ValidateUnitPrice(price);
        if (error is null)
        {
            return price;
        }

        Console.WriteLine($"  Rejected: {error}");
    }
}

static int? ReadQuantity()
{
    while (true)
    {
        Console.Write("Quantity: ");
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        if (!int.TryParse(input, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int quantity))
        {
            Console.WriteLine($"  Rejected: '{input}' is not a whole number.");
            continue;
        }

        string? error = ValidateQuantity(quantity);
        if (error is null)
        {
            return quantity;
        }

        Console.WriteLine($"  Rejected: {error}");
    }
}

// ── Rules: pure decisions, no Console ───────────────────────────
static string? ValidateProductName(string name) => name switch     // ⑯
{
    { Length: < 2 } => "A product name needs at least 2 characters.",
    { Length: > MaxProductNameLength } => $"A product name can be at most {MaxProductNameLength} characters.",
    _ => null,
};

static string? ValidateUnitPrice(decimal price) => price switch     // ⑰
{
    < 0m => "A price can't be negative.",
    > MaxUnitPrice => $"A price above {Money(MaxUnitPrice)} looks like a typo.",
    _ when price != decimal.Round(price, 2) => "A price can have at most two decimals (whole cents).",
    _ => null,
};

static string? ValidateQuantity(int quantity) => quantity switch
{
    < 1 => "Quantity must be at least 1.",
    > MaxQuantity => $"Quantity can be at most {MaxQuantity}.",
    _ => null,
};

static string DecideApproval(decimal total, int quantity) => (total, quantity) switch   // ⑱
{
    ( >= ManagerThreshold, _) => "Manager review required",
    (_, > BulkQuantityThreshold) => "Warehouse check required (bulk quantity)",
    ( >= TeamLeadThreshold, _) => "Team-lead approval required",
    _ => "Auto-approved",
};

// ── Calculations ────────────────────────────────────────────────
static decimal CalculateSubtotal(decimal unitPrice, int quantity) => unitPrice * quantity;

static decimal CalculateVat(decimal subtotal) =>
    Math.Round(subtotal * VatRate, 2, MidpointRounding.AwayFromZero);

static string Money(decimal amount) =>
    amount.ToString("C", CultureInfo.GetCultureInfo(CultureName));  // ⑲

// ── Output ──────────────────────────────────────────────────────
static void PrintWelcome()
{
    Console.WriteLine("Welcome to OrderFlow!");
    Console.WriteLine("Order management, built one lesson at a time.");
    Console.WriteLine("Version: L01 Module 3");
    Console.WriteLine();
}

static void PrintOrderLine(string productName, decimal unitPrice, int quantity)
{
    decimal subtotal = CalculateSubtotal(unitPrice, quantity);
    decimal vat = CalculateVat(subtotal);
    decimal total = subtotal + vat;

    Console.WriteLine($"{quantity} x {productName} @ {Money(unitPrice)} = {Money(subtotal)}");
    Console.WriteLine($"   VAT: {Money(vat)}   Total incl. VAT: {Money(total)}");
    Console.WriteLine($"   Approval: {DecideApproval(total, quantity)}");
}

static void ShowSampleOrders()
{
    PrintOrderLine("USB-C cable", 9.99m, 3);                         // ⑳
    PrintOrderLine("Logo sticker", 0.10m, 3);
    PrintOrderLine("Notebook A5", 19.95m, 4);
    PrintOrderLine("Desk lamp", 49.50m, 5);
    PrintOrderLine("Office chair", 289.00m, 4);
    PrintOrderLine("Logo sticker", 0.10m, 60);
}

static void ShowQuantityTable()
{
    const decimal PenPrice = 1.20m;
    Console.WriteLine($"Pens at {Money(PenPrice)} each:");

    for (int quantity = 10; quantity <= 60; quantity += 10)          // ㉑
    {
        decimal subtotal = CalculateSubtotal(PenPrice, quantity);
        decimal total = subtotal + CalculateVat(subtotal);
        Console.WriteLine($"  {quantity,3} pens -> {Money(total),10}   {DecideApproval(total, quantity)}");
    }
}

static void ShowNumericTraps()
{
    double stickerPrice = 0.10;
    double stickerSubtotal = stickerPrice * 3;
    Console.WriteLine($"double  0.10 x 3 = {stickerSubtotal.ToString(CultureInfo.InvariantCulture)}");
    Console.WriteLine($"double  equals 0.30?  {stickerSubtotal == 0.30}");
    Console.WriteLine($"decimal equals 0.30?  {0.10m * 3 == 0.30m}");
    Console.WriteLine($"7 / 2 = {7 / 2}   7m / 2 = {7m / 2}   (int)2.99m = {(int)2.99m}");
    Console.WriteLine($"Round 2.345 default: {Math.Round(2.345m, 2)}   AwayFromZero: {Math.Round(2.345m, 2, MidpointRounding.AwayFromZero)}");
}
Console.ReadLine(); 