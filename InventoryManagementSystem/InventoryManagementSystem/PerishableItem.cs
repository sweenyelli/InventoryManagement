namespace InventoryManagementSystem;

public class PerishableItem : Item
{
    public int ShelfLifeDays { get; set; }

    public PerishableItem() { }

    public PerishableItem(
        string sku,
        string name,
        string category,
        int shelfCapacity,
        int shelfQty,
        int backOfficeQty,
        string supplierId,
        int shelfLifeDays)
        : base(
            sku, name, category,
            shelfCapacity, shelfQty,
            backOfficeQty, supplierId)
    {
        if (shelfLifeDays <= 0)
            throw new ArgumentException(
                "Shelf life must be positive.");

        ShelfLifeDays = shelfLifeDays;
    }

    public override int CalculateReorderPoint(
        double avgDailySales,
        int leadTimeDays)
    {
        return (int)Math.Ceiling(
            avgDailySales * (leadTimeDays + 1));
    }
}
