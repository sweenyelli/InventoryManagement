namespace InventoryManagementSystem;

public interface IReorderPolicy
{
    int CalculateReorderPoint(
        double avgDailySales,
        int leadTimeDays);
}
