namespace InventoryManagementSystem;

public enum OrderStatus
{
    Pending,
    Received
}

public class PurchaseOrder
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string SupplierId { get; set; } = "";
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Today;
    public DateTime ExpectedArrival { get; set; }

    public OrderStatus Status { get; set; }
        = OrderStatus.Pending;

    public PurchaseOrder() { }

    public PurchaseOrder(
        int id,
        string sku,
        string supplierId,
        int quantity,
        int leadTimeDays)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Order quantity must be positive.");

        if (leadTimeDays <= 0)
            throw new ArgumentException(
                "Lead time must be positive.");

        Id = id;
        Sku = sku;
        SupplierId = supplierId;
        Quantity = quantity;
        OrderDate = DateTime.Today;
        ExpectedArrival = OrderDate.AddDays(leadTimeDays);
        Status = OrderStatus.Pending;
    }

    public void MarkReceived()
    {
        if (Status == OrderStatus.Received)
            throw new InvalidOperationException(
                "Order has already been received.");

        Status = OrderStatus.Received;
    }
}
