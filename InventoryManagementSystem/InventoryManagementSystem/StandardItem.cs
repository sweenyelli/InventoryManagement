namespace InventoryManagementSystem;

public class StandardItem : Item
{
    public StandardItem() { }

    public StandardItem(
        string sku,
        string name,
        string category,
        int shelfCapacity,
        int shelfQty,
        int backOfficeQty,
        string supplierId)
        : base(
            sku, name, category,
            shelfCapacity, shelfQty,
            backOfficeQty, supplierId)
    {
    }

    public override int CalculateReorderPoint(
        double avgDailySales,
        int leadTimeDays)
    {
        return (int)Math.Ceiling(
            avgDailySales * (leadTimeDays + 2));
    }
}
