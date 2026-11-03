using System.Text.Json.Serialization;

namespace InventoryManagementSystem;

public class InventoryStore
{
    public List<Item> Items { get; set; } = new();
    public List<Supplier> Suppliers { get; set; } = new();
    public List<PurchaseOrder> Orders { get; set; } = new();
    public List<StockMovement> Movements { get; set; } = new();

    public int NextOrderId { get; set; } = 1;

    [JsonIgnore]
    public List<Item> RestockAlerts { get; private set; } = new();

    // Reconnect events after loading saved data.
    public void ConnectEvents()
    {
        RestockAlerts = new List<Item>();

        foreach (Item item in Items)
        {
            item.ReorderPointReached -= HandleRestockAlert;
            item.ReorderPointReached += HandleRestockAlert;
        }

        RefreshAlerts();
    }

    private void HandleRestockAlert(Item item)
    {
        if (!RestockAlerts.Contains(item))
            RestockAlerts.Add(item);
    }

    // Search
    public Item? FindItem(string sku)
    {
        return Items.FirstOrDefault(i =>
            i.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
    }

    public Supplier? FindSupplier(string id)
    {
        return Suppliers.FirstOrDefault(s =>
            s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public List<Item> SearchItems(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return new List<Item>(Items);

        return Items.Where(i =>
            i.Sku.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            i.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Average daily sales over the last seven days.
    public double GetAverageDailySales(Item item)
    {
        DateTime now = DateTime.Now;
        DateTime cutoff = now.AddDays(-7);

        int sold = Movements
            .Where(m => m.Sku == item.Sku &&
                        m.Type == MovementType.Sale &&
                        m.Date >= cutoff &&
                        m.Date <= now)
            .Sum(m => m.Quantity);

        return sold / 7.0;
    }

    // Polymorphic reorder calculation.
    public int GetReorderPoint(Item item)
    {
        Supplier? supplier = FindSupplier(item.SupplierId);

        if (supplier == null)
            return item.TemporaryReorderPoint;

        bool hasSales = Movements.Any(m =>
            m.Sku == item.Sku &&
            m.Type == MovementType.Sale &&
            m.Date >= DateTime.Now.AddDays(-7) &&
            m.Date <= DateTime.Now);

        if (!hasSales)
            return item.TemporaryReorderPoint;

        return item.CalculateReorderPoint(
            GetAverageDailySales(item),
            supplier.LeadTimeDays);
    }

    // Alerts and summary counts.
    public void RefreshAlerts()
    {
        RestockAlerts.Clear();

        foreach (Item item in Items)
            item.CheckReorderPoint(GetReorderPoint(item));
    }

    public int CountLowShelfItems() =>
        Items.Count(i => i.IsLowOnShelf());

    public int CountItemsAtReorderPoint()
    {
        RefreshAlerts();
        return RestockAlerts.Count;
    }

    public int CountPendingOrders() =>
        Orders.Count(o => o.Status == OrderStatus.Pending);

    // Item management.
    public void AddItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(item.Sku) ||
            string.IsNullOrWhiteSpace(item.Name))
            throw new ArgumentException("SKU and name are required.");

        if (FindItem(item.Sku) != null)
            throw new InvalidOperationException("SKU already exists.");

        if (item.ShelfCapacity < 0 ||
            item.ShelfQty < 0 ||
            item.BackOfficeQty < 0 ||
            item.ShelfQty > item.ShelfCapacity)
            throw new ArgumentException("Invalid stock values.");

        if (FindSupplier(item.SupplierId) == null)
            throw new InvalidOperationException("Supplier not found.");

        item.ReorderPointReached -= HandleRestockAlert;
        item.ReorderPointReached += HandleRestockAlert;

        Items.Add(item);
        RefreshAlerts();
    }

    public void RemoveItem(string sku)
    {
        Item item = FindItem(sku)
            ?? throw new InvalidOperationException("Item not found.");

        if (Orders.Any(o => o.Sku == item.Sku &&
                            o.Status == OrderStatus.Pending))
            throw new InvalidOperationException(
                "Cannot remove an item with a pending order.");

        item.ReorderPointReached -= HandleRestockAlert;
        Items.Remove(item);
        RestockAlerts.Remove(item);

        RefreshAlerts();
    }

    public void EditItem(
        string sku,
        string name,
        string category,
        int shelfCapacity,
        string supplierId)
    {
        Item item = FindItem(sku)
            ?? throw new InvalidOperationException("Item not found.");

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(category))
            throw new ArgumentException(
                "Name and category are required.");

        if (shelfCapacity < item.ShelfQty)
            throw new ArgumentException(
                "Capacity cannot be below current shelf stock.");

        if (FindSupplier(supplierId) == null)
            throw new InvalidOperationException("Supplier not found.");

        item.Name = name.Trim();
        item.Category = category.Trim();
        item.ShelfCapacity = shelfCapacity;
        item.SupplierId = supplierId;

        RefreshAlerts();
    }

    // Stock operations and movement log.
    public void RecordSale(string sku, int quantity)
    {
        Item item = FindItem(sku)
            ?? throw new InvalidOperationException("Item not found.");

        item.Sell(quantity);

        Movements.Add(new StockMovement(
            item.Sku, quantity, MovementType.Sale));

        RefreshAlerts();
    }

    public void RefillShelf(string sku, int quantity)
    {
        Item item = FindItem(sku)
            ?? throw new InvalidOperationException("Item not found.");

        item.Refill(quantity);

        Movements.Add(new StockMovement(
            item.Sku, quantity, MovementType.Refill));

        RefreshAlerts();
    }

    // Purchase orders.
    public PurchaseOrder CreateOrder(string sku, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Order quantity must be positive.");

        Item item = FindItem(sku)
            ?? throw new InvalidOperationException("Item not found.");

        Supplier supplier = FindSupplier(item.SupplierId)
            ?? throw new InvalidOperationException("Supplier not found.");

        RefreshAlerts();

        if (!RestockAlerts.Contains(item))
            throw new InvalidOperationException(
                "Item is not currently on the restock alert list.");

        var order = new PurchaseOrder(
            NextOrderId,
            item.Sku,
            supplier.Id,
            quantity,
            supplier.LeadTimeDays);

        NextOrderId++;
        Orders.Add(order);

        return order;
    }

    public void ReceiveOrder(int orderId)
    {
        PurchaseOrder order = Orders.FirstOrDefault(
            o => o.Id == orderId)
            ?? throw new InvalidOperationException("Order not found.");

        if (order.Status == OrderStatus.Received)
            throw new InvalidOperationException(
                "Order has already been received.");

        Item item = FindItem(order.Sku)
            ?? throw new InvalidOperationException("Item not found.");

        item.Receive(order.Quantity);

        Movements.Add(new StockMovement(
            item.Sku, order.Quantity, MovementType.Delivery));

        order.MarkReceived();

        RefreshAlerts();
    }

    // Required ranking method: fastest to slowest.
    public List<Item> RankByMovement()
    {
        return Items
            .OrderByDescending(GetAverageDailySales)
            .ThenBy(i => i.Name)
            .ToList();
    }

    public List<Item> GetFastMovers(int count = 3)
    {
        return RankByMovement().Take(Math.Max(0, count)).ToList();
    }

    public List<Item> GetSlowMovers(int count = 3)
    {
        return RankByMovement()
            .OrderBy(GetAverageDailySales)
            .ThenBy(i => i.Name)
            .Take(Math.Max(0, count))
            .ToList();
    }

    // Console output.
    public void PrintItems(string search = "")
    {
        List<Item> results = SearchItems(search);

        if (results.Count == 0)
        {
            Console.WriteLine("No matching items found.");
            return;
        }

        foreach (Item item in results)
        {
            Console.WriteLine(
                $"{item.Sku} | {item.Name} | " +
                $"Shelf: {item.ShelfQty}/{item.ShelfCapacity} | " +
                $"Back office: {item.BackOfficeQty} | " +
                $"Total: {item.TotalStock}");
        }
    }

    public void PrintAlerts()
    {
        RefreshAlerts();

        if (RestockAlerts.Count == 0)
        {
            Console.WriteLine("No items need reordering.");
            return;
        }

        Console.WriteLine("\n--- Restock Alerts ---");

        foreach (Item item in RestockAlerts)
        {
            Console.WriteLine(
                $"{item.Sku} | {item.Name} | " +
                $"Stock: {item.TotalStock} | " +
                $"Reorder point: {GetReorderPoint(item)}");
        }
    }

    public void PrintOrders()
    {
        if (Orders.Count == 0)
        {
            Console.WriteLine("No purchase orders found.");
            return;
        }

        foreach (PurchaseOrder order in Orders.OrderBy(o => o.Id))
        {
            Console.WriteLine(
                $"Order #{order.Id} | {order.Sku} | " +
                $"Quantity: {order.Quantity} | " +
                $"Expected: {order.ExpectedArrival:yyyy-MM-dd} | " +
                $"Status: {order.Status}");
        }
    }

    public void PrintMoverReport()
    {
        Console.WriteLine("\n========== FAST / SLOW MOVERS ==========");
        Console.WriteLine(
            $"{"SKU",-10} {"ITEM",-30} {"SOLD/DAY",10} " +
            $"{"SHELF",7} {"BACK",7} {"REORDER",8}");

        foreach (Item item in RankByMovement())
        {
            Console.WriteLine(
                $"{item.Sku,-10} {item.Name,-30} " +
                $"{GetAverageDailySales(item),10:F2} " +
                $"{item.ShelfQty,7} {item.BackOfficeQty,7} " +
                $"{GetReorderPoint(item),8}");
        }
    }

    public void PrintReportsMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Reports Menu ---");
            Console.WriteLine("1. Fast/Slow Movers");
            Console.WriteLine("2. Fast Movers");
            Console.WriteLine("3. Slow Movers");
            Console.WriteLine("0. Back");
            Console.Write("Select option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    PrintMoverReport();
                    break;

                case "2":
                    PrintSelectedMovers(GetFastMovers());
                    break;

                case "3":
                    PrintSelectedMovers(GetSlowMovers());
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    private void PrintSelectedMovers(List<Item> items)
    {
        foreach (Item item in items)
        {
            Console.WriteLine(
                $"{item.Sku} | {item.Name} | " +
                $"Sales/day: {GetAverageDailySales(item):F2} | " +
                $"Stock: {item.TotalStock}");
        }
    }
}
