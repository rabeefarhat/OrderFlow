// src/OrderFlow.ConsoleApp/Models/OrderStatus.cs
namespace OrderFlow.ConsoleApp.Models;

public enum OrderStatus
{
    Draft,
    Submitted,
    Paid,
    Shipped,
    Cancelled,
}