using System.Text.Json;

namespace InventoryManagementSystem;

public class DataService
{
    private readonly string filePath = "inventory.json";

    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true
    };

    public void Save(InventoryStore store)
    {
        string json = JsonSerializer.Serialize(store, options);

        File.WriteAllText(filePath, json);
    }

    public InventoryStore Load()
    {
        if (!File.Exists(filePath))
        {
            InventoryStore starter = CreateStarterData();
            Save(starter);
            return starter;
        }

        string json = File.ReadAllText(filePath);

        InventoryStore store =
            JsonSerializer.Deserialize<InventoryStore>(
                json, options)
            ?? throw new InvalidOperationException(
                "Could not deserialize inventory data.");

        store.NextOrderId = Math.Max(
            store.NextOrderId,
            store.Orders.Select(o => o.Id)
                .DefaultIfEmpty(0)
                .Max() + 1);

        store.ConnectEvents();

        return store;
    }

    private static InventoryStore CreateStarterData()
    {
        var store = new InventoryStore();

        store.Suppliers.AddRange(new[]
        {
            new Supplier
            {
                Id = "SUP-001",
                Name = "FreshMart Supplies",
                Contact = "freshmart@example.com",
                LeadTimeDays = 2
            },
            new Supplier
            {
                Id = "SUP-002",
                Name = "City Wholesale",
                Contact = "citywholesale@example.com",
                LeadTimeDays = 3
            },
            new Supplier
            {
                Id = "SUP-003",
                Name = "Regional Distributors",
                Contact = "regional@example.com",
                LeadTimeDays = 5
            }
        });

        store.Items.AddRange(new Item[]
        {
            new StandardItem(
                "PRD-001", "Instant Noodles (5-pack)",
                "Food/Grocery", 24, 24, 100, "SUP-002"),

            new StandardItem(
                "PRD-002", "Canned Sardines (155g)",
                "Canned Goods", 30, 20, 60, "SUP-001"),

            new StandardItem(
                "PRD-003", "3-in-1 Coffee Mix (10-sachet bundle)",
                "Beverages", 24, 18, 80, "SUP-002"),

            new StandardItem(
                "PRD-004", "Cooking Oil (500ml pouch)",
                "Cooking Essentials", 20, 12, 40, "SUP-003"),

            new StandardItem(
                "PRD-005", "Laundry Detergent Powder (twin-pack)",
                "Household", 20, 14, 40, "SUP-003"),

            new StandardItem(
                "PRD-006", "Purified Bottled Water (500ml)",
                "Beverages", 36, 30, 120, "SUP-001"),

            new PerishableItem(
                "PRD-007", "Evaporated Milk (300ml can)",
                "Canned Goods", 24, 12, 30, "SUP-001", 180),

            new PerishableItem(
                "PRD-008", "Biscuits (10-pack)",
                "Snacks", 24, 16, 30, "SUP-002", 90)
        });

        // Seven days of sample sales history.
        foreach (Item item in store.Items)
        {
            for (int day = 1; day <= 7; day++)
            {
                int quantity = item.Sku switch
                {
                    "PRD-001" => 6,
                    "PRD-002" => 3,
                    "PRD-003" => 5,
                    "PRD-004" => 1,
                    "PRD-005" => 2,
                    "PRD-006" => 4,
                    "PRD-007" => 2,
                    _ => 1
                };

                store.Movements.Add(new StockMovement
                {
                    Sku = item.Sku,
                    Date = DateTime.Now.AddDays(-day)
                        .AddMinutes(1),
                    Quantity = quantity,
                    Type = MovementType.Sale
                });
            }
        }

        store.ConnectEvents();

        return store;
    }
}
