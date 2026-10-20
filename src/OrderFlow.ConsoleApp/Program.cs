// src/OrderFlow.ConsoleApp/Program.cs
// OrderFlow — P01.5: an in-memory order store with LINQ search.

using System.Globalization;
using OrderFlow.ConsoleApp.Models;
using OrderFlow.ConsoleApp.Store;                                             // ①

// ── Settings: every business number has a name ──────────────────
const string CultureName = "nl-BE";
const decimal TeamLeadThreshold = 250m;
const decimal ManagerThreshold = 1_000m;
const int BulkQuantityThreshold = 50;
const string WalkInCustomer = "Walk-in customer";

// ── The program itself ──────────────────────────────────────────
InMemoryOrderStore store = CreateSampleStore();                               // ②
PrintWelcome();
RunMenu(store);
Console.WriteLine("Goodbye from OrderFlow.");

// ── Menu ────────────────────────────────────────────────────────
static void RunMenu(InMemoryOrderStore store)                                 // ③
{
    while (true)
    {
        PrintMenu();
        string? line = Console.ReadLine();
        if (line is null)
        {
            return;
        }

        switch (line.Trim())
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
            case "5":
                ShowMultiLineOrder();
                break;
            case "6":
                ShowObjectRules();
                break;
            case "7":
                ShowReferencesAndValues();
                break;
            case "8":
                SearchCustomerOrders(store);
                break;
            case "9":
                ShowStoreReport(store);
                break;
            case "0":
                return;
            default:
                Console.WriteLine($"  Rejected: '{line.Trim()}' is not a menu option. Type one of the numbers above.");
                break;
        }

        Console.WriteLine();
    }
}

static void PrintMenu()
{
    string[] options =                                                         // ④
    [
        "1  Price a new order line",
        "2  Show the sample orders",
        "3  Show a quantity table (approval boundaries)",
        "4  Show the numeric traps (decimal vs double)",
        "5  Build a multi-line order",
        "6  Try to break the order rules",
        "7  References vs values",
        "8  Search a customer's orders",
        "9  Store report (sorted, grouped, by status)",
        "0  Exit",
    ];

    Console.WriteLine("Menu");
    foreach (string option in options)
    {
        Console.WriteLine($"  {option}");
    }

    Console.Write("Your choice: ");
}

// ── Use case: price one order line typed by the user ────────────
static void PriceNewOrderLine()
{
    Console.WriteLine("Press Enter on an empty line at any question to cancel.");

    if (ReadProductName() is not string productName)
    {
        PrintCancelled();
        return;
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

    // The input was validated above, so these constructors will not throw.
    Product product = new(productName, unitPrice, stockQuantity: 0);
    Order order = new(WalkInCustomer);
    order.AddItem(product, quantity);
    PrintOrder(order);
}

static void PrintCancelled() => Console.WriteLine("Cancelled - nothing was priced.");

// ── Input: read, convert, validate, ask again ───────────────────
static string? ReadProductName()
{
    while (true)
    {
        Console.Write("Product name: ");
        string? input = Console.ReadLine()?.Trim();
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
    NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    while (true)
    {
        Console.Write("Unit price in euro, with a comma (for example 9,99): ");
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        if (!decimal.TryParse(input, style, belgian, out decimal price))
        {
            Console.WriteLine($"  Rejected: '{input}' is not a price. Use digits and one comma, like 9,99.");
            continue;
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

// ── Boundary rules: friendly messages, limits owned by the classes ─
static string? ValidateProductName(string name) => name switch
{
    { Length: < Product.MinNameLength } => $"A product name needs at least {Product.MinNameLength} characters.",
    { Length: > Product.MaxNameLength } => $"A product name can be at most {Product.MaxNameLength} characters.",
    _ => null,
};

static string? ValidateUnitPrice(decimal price) => price switch
{
    < 0m => "A price can't be negative.",
    > Product.MaxUnitPrice => $"A price above {Money(Product.MaxUnitPrice)} looks like a typo.",
    _ when price != decimal.Round(price, 2) => "A price can have at most two decimals (whole cents).",
    _ => null,
};

static string? ValidateQuantity(int quantity) => quantity switch
{
    < OrderItem.MinQuantity => $"Quantity must be at least {OrderItem.MinQuantity}.",
    > OrderItem.MaxQuantity => $"Quantity can be at most {OrderItem.MaxQuantity}.",
    _ => null,
};

static string DecideApproval(decimal total, int quantity) => (total, quantity) switch
{
    ( >= ManagerThreshold, _) => "Manager review required",
    (_, > BulkQuantityThreshold) => "Warehouse check required (bulk quantity)",
    ( >= TeamLeadThreshold, _) => "Team-lead approval required",
    _ => "Auto-approved",
};

static string Money(decimal amount) =>
    amount.ToString("C", CultureInfo.GetCultureInfo(CultureName));

// ── Output ──────────────────────────────────────────────────────
static void PrintWelcome()
{
    Console.WriteLine("Welcome to OrderFlow!");
    Console.WriteLine("Order management, built one lesson at a time.");
    Console.WriteLine("Version: L01 Module 5");
    Console.WriteLine();
}

static void PrintOrder(Order order)
{
    Console.WriteLine($"Order for {order.CustomerName} - status {order.Status}");
    foreach (OrderItem item in order.Items)
    {
        Console.WriteLine($"  {item.Quantity,5} x {item.ProductName,-20} @ {Money(item.UnitPrice),12} = {Money(item.Subtotal),12}");
    }

    decimal total = order.CalculateTotal();
    Console.WriteLine($"  Subtotal: {Money(order.CalculateSubtotal())}   VAT: {Money(order.CalculateVat())}   Total incl. VAT: {Money(total)}");
    Console.WriteLine($"  Approval: {DecideApproval(total, order.TotalQuantity)}");
}

static void PrintSingleLineOrder(string productName, decimal unitPrice, int quantity)
{
    Product product = new(productName, unitPrice, stockQuantity: 1_000);
    Order order = new(WalkInCustomer);
    order.AddItem(product, quantity);
    PrintOrder(order);
}

static void ShowSampleOrders()
{
    PrintSingleLineOrder("USB-C cable", 9.99m, 3);
    PrintSingleLineOrder("Logo sticker", 0.10m, 3);
    PrintSingleLineOrder("Notebook A5", 19.95m, 4);
    PrintSingleLineOrder("Desk lamp", 49.50m, 5);
    PrintSingleLineOrder("Office chair", 289.00m, 4);
    PrintSingleLineOrder("Logo sticker", 0.10m, 60);
}

static void ShowQuantityTable()
{
    Product pen = new("Ballpoint pen", 1.20m, stockQuantity: 500);
    Console.WriteLine($"Pens at {Money(pen.UnitPrice)} each:");

    for (int quantity = 10; quantity <= 60; quantity += 10)
    {
        Order order = new(WalkInCustomer);
        order.AddItem(pen, quantity);
        decimal total = order.CalculateTotal();
        Console.WriteLine($"  {quantity,3} pens -> {Money(total),10}   {DecideApproval(total, order.TotalQuantity)}");
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

// ── Module 4: objects in action ─────────────────────────────────
static void ShowMultiLineOrder()
{
    Product cable = new("USB-C cable", 9.99m, stockQuantity: 120);
    Product notebook = new("Notebook A5", 19.95m, stockQuantity: 300);
    Product lamp = new("Desk lamp", 49.50m, stockQuantity: 25);

    Order order = new("Atelier Janssens");
    order.AddItem(cable, 3);
    order.AddItem(notebook, 4);
    order.AddItem(lamp, 2);

    PrintOrder(order);
    order.Submit();
    Console.WriteLine($"  After Submit(): status {order.Status}");
}

static void ShowObjectRules()
{
    Product cable = new("USB-C cable", 9.99m, stockQuantity: 120);
    Order order = new(WalkInCustomer);

    Console.WriteLine("1) Add an item with quantity 0:");
    try
    {
        order.AddItem(cable, 0);
        Console.WriteLine("   PROBLEM: the order accepted it.");
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine($"   Refused by the object, as expected. Lines in the order: {order.Items.Count}  [{ex.GetType().Name}]");
    }

    Console.WriteLine("2) Submit an empty order:");
    try
    {
        order.Submit();
        Console.WriteLine("   PROBLEM: an empty order was submitted.");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"   Refused, as expected. Status is still {order.Status}  [{ex.GetType().Name}]");
    }

    Console.WriteLine("3) Set a negative price on a product:");
    try
    {
        cable.UnitPrice = -5m;
        Console.WriteLine("   PROBLEM: the product accepted a negative price.");
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine($"   Refused, as expected. Price is still {Money(cable.UnitPrice)}  [{ex.GetType().Name}]");
    }

    Console.WriteLine("4) Add an item after the order was submitted:");
    order.AddItem(cable, 1);
    order.Submit();
    try
    {
        order.AddItem(cable, 1);
        Console.WriteLine("   PROBLEM: a submitted order was changed.");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"   Refused, as expected. Lines in the order: {order.Items.Count}  [{ex.GetType().Name}]");
    }
}

static void ShowReferencesAndValues()
{
    Product lamp = new("Desk lamp", 49.50m, stockQuantity: 25);
    Product sameLamp = lamp;
    decimal oldPrice = lamp.UnitPrice;

    Order order = new(WalkInCustomer);
    order.AddItem(lamp, 1);
    OrderItem line = order.Items[0];

    sameLamp.UnitPrice = 59.50m;

    Console.WriteLine($"  Same object?            {object.ReferenceEquals(lamp, sameLamp)}");
    Console.WriteLine($"  lamp.UnitPrice          {Money(lamp.UnitPrice)}   changed through sameLamp");
    Console.WriteLine($"  oldPrice                {Money(oldPrice)}   a decimal is copied");
    Console.WriteLine($"  line.UnitPrice          {Money(line.UnitPrice)}   frozen when the line was added");
    Console.WriteLine($"  line.Product.UnitPrice  {Money(line.Product.UnitPrice)}   same Product object as lamp");
}

// ── Module 5: the store and LINQ ────────────────────────────────
static InMemoryOrderStore CreateSampleStore()                                 // ⑤
{
    Product cable = new("USB-C cable", 9.99m, stockQuantity: 120);
    Product notebook = new("Notebook A5", 19.95m, stockQuantity: 300);
    Product lamp = new("Desk lamp", 49.50m, stockQuantity: 25);
    Product chair = new("Office chair", 289.00m, stockQuantity: 10);
    Product pen = new("Ballpoint pen", 1.20m, stockQuantity: 500);

    Order janssensSmall = new("Atelier Janssens");
    janssensSmall.AddItem(cable, 3);
    janssensSmall.AddItem(notebook, 4);
    janssensSmall.AddItem(lamp, 2);
    janssensSmall.Submit();

    Order janssensChairs = new("Atelier Janssens");
    janssensChairs.AddItem(chair, 4);

    Order peetersPens = new("Bakkerij Peeters");
    peetersPens.AddItem(pen, 20);
    peetersPens.Submit();

    Order peetersNotebooks = new("Bakkerij Peeters");
    peetersNotebooks.AddItem(notebook, 2);

    Order verhaegen = new("Studio Verhaegen");
    verhaegen.AddItem(lamp, 1);
    verhaegen.AddItem(cable, 1);
    verhaegen.Submit();

    InMemoryOrderStore store = new();
    Order[] samples = [janssensSmall, janssensChairs, peetersPens, peetersNotebooks, verhaegen];   // ⑥
    foreach (Order order in samples)
    {
        store.Add(order);
    }

    return store;
}

static void SearchCustomerOrders(InMemoryOrderStore store)                    // ⑦
{
    Console.Write("Customer name (Enter to cancel): ");
    string? input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input))
    {
        Console.WriteLine("Cancelled - no search was run.");
        return;
    }

    IReadOnlyList<Order> orders = store.FindByCustomer(input);
    if (orders.Count == 0)                                                    // ⑧
    {
        Console.WriteLine($"  No orders found for '{input}'. Check the spelling, or choose 9 to see every customer.");
        return;
    }

    Console.WriteLine($"  {orders.Count} order(s) for {orders[0].CustomerName}, highest value first:");
    foreach (Order order in orders)
    {
        PrintOrderSummaryLine(order);
    }

    Order? highest = store.FindHighestValueOrder(input);
    if (highest is not null)                                                  // ⑨
    {
        Console.WriteLine("  Highest-value order in full:");
        PrintOrder(highest);
    }
}

static void ShowStoreReport(InMemoryOrderStore store)                         // ⑩
{
    Console.WriteLine($"All {store.Count} orders, highest total first:");
    foreach (Order order in store.AllByTotalDescending())
    {
        PrintOrderSummaryLine(order);
    }

    Console.WriteLine("Per customer:");
    foreach (CustomerSummary summary in store.SummariseByCustomer())
    {
        Console.WriteLine($"  {summary.CustomerName,-20} {summary.OrderCount,2} order(s)   {Money(summary.TotalSpent),12}");
    }

    Console.WriteLine("Orders per status:");
    foreach (KeyValuePair<OrderStatus, int> entry in store.CountByStatus())   // ⑪
    {
        Console.WriteLine($"  {entry.Key,-10} {entry.Value}");
    }

    IReadOnlyList<Order> submitted = store.FindByStatus(OrderStatus.Submitted);
    Console.WriteLine($"Submitted orders waiting for payment: {submitted.Count}");
    foreach (Order order in submitted)
    {
        PrintOrderSummaryLine(order);
    }
}

static void PrintOrderSummaryLine(Order order) =>
    Console.WriteLine($"  {order.CustomerName,-20} {order.Status,-10} {order.Items.Count} line(s) {Money(order.CalculateTotal()),12}");