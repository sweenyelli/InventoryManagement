namespace InventoryManagementSystem;

public class Admin : Staff
{
    public override void ShowMenu(InventoryStore store)
    {
        while (true)
        {
            store.RefreshAlerts();

            Console.WriteLine("\n--- Main Menu (Admin) ---");
            Console.WriteLine(
                $"Alerts: {store.CountLowShelfItems()} low on shelf | " +
                $"{store.CountItemsAtReorderPoint()} at reorder point | " +
                $"{store.CountPendingOrders()} orders pending");

            Console.WriteLine(
                "1. Search Items\n" +
                "2. Record Sale\n" +
                "3. Refill Shelf\n" +
                "4. Restock Alerts\n" +
                "5. Create Purchase Order\n" +
                "6. Receive Delivery\n" +
                "7. View Orders\n" +
                "8. Add Item\n" +
                "9. Edit Item\n" +
                "10. Remove Item\n" +
                "11. Manage Suppliers\n" +
                "12. Reports\n" +
                "0. Exit");

            Console.Write("Select option: ");
            string choice = Console.ReadLine() ?? "";

            try
            {
                switch (choice)
                {
                    case "1":
                        Console.Write("Enter name or SKU: ");
                        store.PrintItems(Console.ReadLine() ?? "");
                        break;

                    case "2":
                        Console.Write("Enter SKU: ");
                        store.RecordSale(
                            Console.ReadLine() ?? "",
                            ReadPositiveQuantity("Quantity sold: "));
                        Console.WriteLine("Sale recorded.");
                        break;

                    case "3":
                        Console.Write("Enter SKU: ");
                        store.RefillShelf(
                            Console.ReadLine() ?? "",
                            ReadPositiveQuantity("Refill quantity: "));
                        Console.WriteLine("Shelf refilled.");
                        break;

                    case "4":
                        store.PrintAlerts();
                        break;

                    case "5":
                        store.PrintAlerts();
                        Console.Write("Enter SKU: ");
                        string sku = Console.ReadLine() ?? "";

                        PurchaseOrder order = store.CreateOrder(
                            sku,
                            ReadPositiveQuantity("Order quantity: "));

                        Console.WriteLine(
                            $"Order #{order.Id} created. " +
                            $"Expected arrival: " +
                            $"{order.ExpectedArrival:yyyy-MM-dd}");
                        break;

                    case "6":
                        store.PrintOrders();
                        store.ReceiveOrder(
                            ReadPositiveQuantity("Order ID: "));
                        Console.WriteLine("Delivery received.");
                        break;

                    case "7":
                        store.PrintOrders();
                        break;

                    case "8":
                        AddItem(store);
                        break;

                    case "9":
                        EditItem(store);
                        break;

                    case "10":
                        Console.Write("SKU to remove: ");
                        store.RemoveItem(Console.ReadLine() ?? "");
                        Console.WriteLine("Item removed.");
                        break;

                    case "11":
                        ManageSuppliers(store);
                        break;

                    case "12":
                        store.PrintReportsMenu();
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Invalid menu option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void AddItem(InventoryStore store)
    {
        Console.Write("SKU: ");
        string sku = ReadRequiredText();

        Console.Write("Item name: ");
        string name = ReadRequiredText();

        Console.Write("Category: ");
        string category = ReadRequiredText();

        int capacity = ReadNonNegativeInt("Shelf capacity: ");
        int shelf = ReadNonNegativeInt("Initial shelf quantity: ");
        int back = ReadNonNegativeInt("Back-office quantity: ");

        Console.WriteLine("\nAvailable suppliers:");
        foreach (Supplier supplier in store.Suppliers)
            Console.WriteLine(supplier);

        Console.Write("Supplier ID: ");
        string supplierId = ReadRequiredText();

        Console.WriteLine("1. Standard item");
        Console.WriteLine("2. Perishable item");
        Console.Write("Select type: ");
        string type = Console.ReadLine() ?? "";

        Item item;

        if (type == "1")
        {
            item = new StandardItem(
                sku, name, category, capacity,
                shelf, back, supplierId);
        }
        else if (type == "2")
        {
            int shelfLife = ReadPositiveQuantity(
                "Shelf life in days: ");

            item = new PerishableItem(
                sku, name, category, capacity,
                shelf, back, supplierId, shelfLife);
        }
        else
        {
            Console.WriteLine("Invalid item type.");
            return;
        }

        string temp = ReadOptionalText(
            "Temporary reorder point (default 10): ");

        if (temp != "" &&
            int.TryParse(temp, out int point) && point >= 0)
        {
            item.TemporaryReorderPoint = point;
        }
        else if (temp != "")
        {
            Console.WriteLine("Invalid value; using default.");
        }

        store.AddItem(item);
        Console.WriteLine("Item added successfully.");
    }

    private static void EditItem(InventoryStore store)
    {
        Console.Write("SKU to edit: ");
        string sku = ReadRequiredText();

        Item item = store.FindItem(sku)
            ?? throw new InvalidOperationException("Item not found.");

        Console.Write($"Name ({item.Name}): ");
        string name = ReadOptionalText("New name: ");
        if (name == "") name = item.Name;

        Console.Write($"Category ({item.Category}): ");
        string category = ReadOptionalText("New category: ");
        if (category == "") category = item.Category;

        Console.Write($"Shelf capacity ({item.ShelfCapacity}): ");
        string capacityInput = ReadOptionalText("New capacity: ");

        int capacity = item.ShelfCapacity;

        if (capacityInput != "" &&
            (!int.TryParse(capacityInput, out capacity) ||
             capacity < item.ShelfQty))
        {
            throw new ArgumentException(
                "Capacity must be a number at least equal to shelf stock.");
        }

        Console.WriteLine("Available suppliers:");
        foreach (Supplier supplier in store.Suppliers)
            Console.WriteLine(supplier);

        Console.Write($"Supplier ({item.SupplierId}): ");
        string supplierId = ReadOptionalText("New supplier ID: ");
        if (supplierId == "") supplierId = item.SupplierId;

        store.EditItem(sku, name, category, capacity, supplierId);
        Console.WriteLine("Item updated.");
    }

    private static void ManageSuppliers(InventoryStore store)
    {
        Console.WriteLine("\n--- Supplier Management ---");
        Console.WriteLine("1. List suppliers");
        Console.WriteLine("2. Add supplier");
        Console.WriteLine("3. Edit supplier");
        Console.WriteLine("4. Remove supplier");
        Console.Write("Choice: ");

        switch (Console.ReadLine())
        {
            case "1":
                foreach (Supplier supplier in store.Suppliers)
                    Console.WriteLine(
                        $"{supplier.Id} | {supplier.Name} | " +
                        $"{supplier.Contact} | " +
                        $"{supplier.LeadTimeDays} days");
                break;

            case "2":
                Console.Write("Supplier ID: ");
                string id = ReadRequiredText();

                if (store.FindSupplier(id) != null)
                    throw new InvalidOperationException(
                        "Supplier ID already exists.");

                Console.Write("Supplier name: ");
                string name = ReadRequiredText();

                Console.Write("Contact details: ");
                string contact = ReadRequiredText();

                int lead = ReadPositiveQuantity("Lead time in days: ");

                store.Suppliers.Add(new Supplier
                {
                    Id = id,
                    Name = name,
                    Contact = contact,
                    LeadTimeDays = lead
                });

                Console.WriteLine("Supplier added.");
                break;

            case "3":
                Console.Write("Supplier ID to edit: ");
                Supplier edit = store.FindSupplier(
                    Console.ReadLine() ?? "")
                    ?? throw new InvalidOperationException(
                        "Supplier not found.");

                string newName = ReadOptionalText(
                    $"New name ({edit.Name}): ");
                string newContact = ReadOptionalText(
                    $"New contact ({edit.Contact}): ");
                string newLead = ReadOptionalText(
                    $"New lead time ({edit.LeadTimeDays}): ");

                if (newName != "") edit.Name = newName;
                if (newContact != "") edit.Contact = newContact;

                if (newLead != "")
                {
                    if (!int.TryParse(newLead, out int days) || days <= 0)
                        throw new ArgumentException(
                            "Lead time must be positive.");

                    edit.LeadTimeDays = days;
                }

                store.RefreshAlerts();
                Console.WriteLine("Supplier updated.");
                break;

            case "4":
                Console.Write("Supplier ID to remove: ");
                string removeId = ReadRequiredText();

                Supplier remove = store.FindSupplier(removeId)
                    ?? throw new InvalidOperationException(
                        "Supplier not found.");

                if (store.Items.Any(i => i.SupplierId == removeId) ||
                    store.Orders.Any(o => o.SupplierId == removeId))
                {
                    throw new InvalidOperationException(
                        "Supplier is linked to items or orders.");
                }

                store.Suppliers.Remove(remove);
                Console.WriteLine("Supplier removed.");
                break;

            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }

    private static string ReadRequiredText()
    {
        while (true)
        {
            string value = Console.ReadLine() ?? "";

            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            Console.Write("This field is required. Try again: ");
        }
    }

    private static string ReadOptionalText(string prompt)
    {
        Console.Write(prompt);
        return (Console.ReadLine() ?? "").Trim();
    }

    private static int ReadNonNegativeInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int value) &&
                value >= 0)
                return value;

            Console.WriteLine("Enter a whole number of zero or more.");
        }
    }
}
