namespace InventoryManagementSystem;

public class Staff : User
{
    public override void ShowMenu(InventoryStore store)
    {
        while (true)
        {
            store.RefreshAlerts();

            Console.WriteLine("\n--- Main Menu (Staff) ---");
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
                        string saleSku = Console.ReadLine() ?? "";
                        int saleQty = ReadPositiveQuantity(
                            "Quantity sold: ");

                        store.RecordSale(saleSku, saleQty);
                        Console.WriteLine("Sale recorded.");
                        break;

                    case "3":
                        Console.Write("Enter SKU: ");
                        string refillSku = Console.ReadLine() ?? "";
                        int refillQty = ReadPositiveQuantity(
                            "Refill quantity: ");

                        store.RefillShelf(refillSku, refillQty);
                        Console.WriteLine("Shelf refilled.");
                        break;

                    case "4":
                        store.PrintAlerts();
                        break;

                    case "5":
                        store.PrintAlerts();

                        Console.Write("Enter SKU to order: ");
                        string orderSku = Console.ReadLine() ?? "";
                        int orderQty = ReadPositiveQuantity(
                            "Order quantity: ");

                        PurchaseOrder order =
                            store.CreateOrder(orderSku, orderQty);

                        Console.WriteLine(
                            $"Order #{order.Id} created. " +
                            $"Expected arrival: " +
                            $"{order.ExpectedArrival:yyyy-MM-dd}");
                        break;

                    case "6":
                        store.PrintOrders();
                        int orderId = ReadPositiveQuantity(
                            "Enter order ID to receive: ");

                        store.ReceiveOrder(orderId);
                        Console.WriteLine("Delivery received.");
                        break;

                    case "7":
                        store.PrintOrders();
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

    protected static int ReadPositiveQuantity(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int quantity) &&
                quantity > 0)
            {
                return quantity;
            }

            Console.WriteLine(
                "Enter a whole number greater than zero.");
        }
    }
}
