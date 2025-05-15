namespace Tutorial9.Model;

public class Order
{
    int IdOrder { get; set; }
    int IdProduct { get; set; }
    int Amount { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime FulfilledAt { get; set; }
    
}