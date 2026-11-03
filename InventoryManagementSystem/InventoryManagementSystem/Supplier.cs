namespace InventoryManagementSystem;

public class Supplier
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public int LeadTimeDays { get; set; }

    public override string ToString()
    {
        return $"{Id} - {Name} ({LeadTimeDays} days)";
    }
}
