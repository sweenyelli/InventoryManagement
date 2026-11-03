using System.Text.Json.Serialization;

namespace InventoryManagementSystem;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(StandardItem), "standard")]
[JsonDerivedType(typeof(PerishableItem), "perishable")]
public abstract class Item : IReorderPolicy
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string SupplierId { get; set; } = "";

    public int ShelfCapacity { get; set; }

    [JsonInclude]
    public int ShelfQty { get; private set; }

    [JsonInclude]
    public int BackOfficeQty { get; private set; }

    public int TemporaryReorderPoint { get; set; } = 10;

    public int TotalStock => ShelfQty + BackOfficeQty;

    public event Action<Item>? ReorderPointReached;

    protected Item() { }

    protected Item(
        string sku,
        string name,
        string category,
        int shelfCapacity,
        int shelfQty,
        int backOfficeQty,
        string supplierId)
    {
        if (string.IsNullOrWhiteSpace(sku) ||
            string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "SKU and name are required.");
        }

        if (shelfCapacity < 0 ||
            shelfQty < 0 ||
            backOfficeQty < 0 ||
            shelfQty > shelfCapacity)
        {
            throw new ArgumentException(
                "Invalid stock quantities or shelf capacity.");
        }

        Sku = sku.Trim();
        Name = name.Trim();
        Category = category.Trim();
        ShelfCapacity = shelfCapacity;
        ShelfQty = shelfQty;
        BackOfficeQty = backOfficeQty;
        SupplierId = supplierId;
    }

    public bool IsLowOnShelf()
    {
        return ShelfQty <= Math.Max(1, ShelfCapacity / 4);
    }

    public abstract int CalculateReorderPoint(
        double avgDailySales,
        int leadTimeDays);

    public void Sell(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Sale quantity must be positive.");

        if (quantity > ShelfQty)
            throw new InvalidOperationException(
                "Sale exceeds available shelf stock.");

        ShelfQty -= quantity;
    }

    public void Refill(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Refill quantity must be positive.");

        if (quantity > BackOfficeQty)
            throw new InvalidOperationException(
                "Not enough back-office stock.");

        if (quantity > ShelfCapacity - ShelfQty)
            throw new InvalidOperationException(
                "Refill exceeds remaining shelf capacity.");

        BackOfficeQty -= quantity;
        ShelfQty += quantity;
    }

    public void Receive(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Delivery quantity must be positive.");

        BackOfficeQty += quantity;
    }

    public void CheckReorderPoint(int reorderPoint)
    {
        if (TotalStock <= reorderPoint)
            ReorderPointReached?.Invoke(this);
    }

    public void SetStock(int shelfQty, int backOfficeQty)
    {
        if (shelfQty < 0 ||
            backOfficeQty < 0 ||
            shelfQty > ShelfCapacity)
        {
            throw new ArgumentException(
                "Invalid stock values.");
        }

        ShelfQty = shelfQty;
        BackOfficeQty = backOfficeQty;
    }
}
