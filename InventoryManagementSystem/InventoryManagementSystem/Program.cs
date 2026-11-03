namespace InventoryManagementSystem;

internal class Program
{
    static void Main()
    {
        Console.Title = "Inventory Management System";

        var dataService = new DataService();
        InventoryStore store;

        try
        {
            store = dataService.Load();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error loading inventory: {ex.Message}");

            Console.WriteLine(
                "If inventory.json is corrupted, back it up before " +
                "creating a new starter file.");

            return;
        }

        bool exit = false;

        while (!exit)
        {
            Console.WriteLine(
                "\n=== Inventory Management System ===");

            Console.WriteLine("1. Staff");
            Console.WriteLine("2. Admin");
            Console.WriteLine("0. Exit");
            Console.Write("Select role: ");

            string choice = Console.ReadLine() ?? "";

            User? user = choice switch
            {
                "1" => new Staff { Name = "Staff" },
                "2" => new Admin { Name = "Admin" },
                "0" => null,
                _ => null
            };

            if (choice == "0")
            {
                exit = true;
                continue;
            }

            if (user == null)
            {
                Console.WriteLine(
                    "Invalid choice. Select 1, 2, or 0.");
                continue;
            }

            user.ShowMenu(store);

            try
            {
                dataService.Save(store);
                Console.WriteLine("Inventory saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error saving inventory: {ex.Message}");
            }
        }

        Console.WriteLine("Thank you for using the system.");
    }
}
