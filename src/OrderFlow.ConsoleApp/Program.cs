// src/OrderFlow.ConsoleApp/Program.cs
const string appName = "OrderFlow";
var startedAt = DateTimeOffset.Now;

Console.WriteLine($"Welcome to {appName}!");
Console.WriteLine($"Started at {startedAt:HH:mm:ss}.");
Console.WriteLine("Press Enter to exit.");
Console.ReadLine();