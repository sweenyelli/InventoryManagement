namespace InventoryManagementSystem;

public enum MovementType
{
    Sale,
    Refill,
    Delivery
}

public class StockMovement
{
    public string Sku { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public int Quantity { get; set; }
    public MovementType Type { get; set; }

    public StockMovement() { }

    public StockMovement(
        string sku,
        int quantity,
        MovementType type)
    {
        Sku = sku;
        Quantity = quantity;
        Type = type;
        Date = DateTime.Now;
    }
}
