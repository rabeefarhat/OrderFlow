// src/OrderFlow.ConsoleApp/Program.cs
// OrderFlow — P01.2: decimal arithmetic and order subtotals.

using System.Globalization;                                          // ①

CultureInfo euro = CultureInfo.GetCultureInfo("nl-BE");              // ②

Console.WriteLine("Welcome to OrderFlow!");
Console.WriteLine("Order management, built one lesson at a time.");
Console.WriteLine("Version: L01 Module 2");
Console.WriteLine();                                                 // ③

// ── Order 1: one product line ───────────────────────────────────
string productName1 = "USB-C cable";                                 // ④
decimal unitPrice1 = 9.99m;                                          // ⑤
int quantity1 = 3;
decimal subtotal1 = unitPrice1 * quantity1;                          // ⑥
Console.WriteLine($"Order 1: {quantity1} x {productName1} @ {unitPrice1.ToString("C", euro)} = {subtotal1.ToString("C", euro)}"); // ⑦

// ── Order 2: cheap stickers (the amount double gets wrong) ──────
string productName2 = "Logo sticker";
decimal unitPrice2 = 0.10m;
int quantity2 = 3;
decimal subtotal2 = unitPrice2 * quantity2;
Console.WriteLine($"Order 2: {quantity2} x {productName2} @ {unitPrice2.ToString("C", euro)} = {subtotal2.ToString("C", euro)}");

// ── Order 3: with 21% Belgian VAT, rounded to cents ─────────────
string productName3 = "Notebook A5";
decimal unitPrice3 = 19.95m;
int quantity3 = 4;
const decimal VatRate = 0.21m;                                       // ⑧
decimal subtotal3 = unitPrice3 * quantity3;
decimal vat3 = Math.Round(subtotal3 * VatRate, 2, MidpointRounding.AwayFromZero); // ⑨
decimal total3 = subtotal3 + vat3;
Console.WriteLine($"Order 3: {quantity3} x {productName3} @ {unitPrice3.ToString("C", euro)} = {subtotal3.ToString("C", euro)}");
Console.WriteLine($"         VAT 21%: {vat3.ToString("C", euro)}   Total incl. VAT: {total3.ToString("C", euro)}");

decimal allSubtotals = subtotal1 + subtotal2 + subtotal3;
Console.WriteLine($"Sum of the three subtotals: {allSubtotals.ToString("C", euro)}");
Console.WriteLine();

// ── ❌ Negative check: the same sticker maths with double ────────
double stickerPriceDouble = 0.10;                                    // ⑩
double stickerSubtotalDouble = stickerPriceDouble * 3;
Console.WriteLine($"double  0.10 x 3 = {stickerSubtotalDouble.ToString(CultureInfo.InvariantCulture)}"); // ⑪
Console.WriteLine($"double  equals 0.30?  {stickerSubtotalDouble == 0.30}");
Console.WriteLine($"decimal equals 0.30?  {subtotal2 == 0.30m}");
Console.WriteLine();

// ── Traps worth seeing once ─────────────────────────────────────
int itemsPerBox = 7 / 2;                                             // ⑫
decimal exactPerBox = 7m / 2;
int truncatedPrice = (int)2.99m;                                     // ⑬
decimal bankersRounding = Math.Round(2.345m, 2);                     // ⑭
decimal schoolRounding = Math.Round(2.345m, 2, MidpointRounding.AwayFromZero);
Console.WriteLine($"7 / 2 = {itemsPerBox}   7m / 2 = {exactPerBox}   (int)2.99m = {truncatedPrice}");
Console.WriteLine($"Round 2.345 default: {bankersRounding}   AwayFromZero: {schoolRounding}");

string? customerNote = null;                                         // ⑮
Console.WriteLine($"Customer note: {customerNote ?? "(no note)"}");
Console.WriteLine();

// ── 🔒 Guard: refuse values that make no business sense ─────────
decimal suspiciousPrice = -5.00m;
int suspiciousQuantity = 0;
bool priceIsValid = suspiciousPrice >= 0m;                           // ⑯
bool quantityIsValid = suspiciousQuantity >= 1;
if (!priceIsValid || !quantityIsValid)                               // ⑰
{
    Console.WriteLine($"Rejected line: price {suspiciousPrice.ToString("C", euro)}, quantity {suspiciousQuantity} (price must not be negative, quantity must be at least 1).");
}