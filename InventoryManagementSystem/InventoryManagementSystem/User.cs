namespace InventoryManagementSystem;

public abstract class User
{
    public string Name { get; set; } = "";

    public abstract void ShowMenu(InventoryStore store);
}
