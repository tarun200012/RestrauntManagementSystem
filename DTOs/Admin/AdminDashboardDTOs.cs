namespace RestaurantAPI.DTOs.Admin
{
    public class OrdersOverviewDto
    {
        public int TotalOrders { get; set; }
        public int OrdersToday { get; set; }
        public int OrdersThisWeek { get; set; }
        public int OrdersThisMonth { get; set; }
    }

    public class RevenueDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal CancelRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public string TopSellingItems { get; set; } = string.Empty;
        public decimal TopItemRevenue { get; set; }
        public decimal? Profitability { get; set; } // nullable because SP may omit
    }
    public class MenuItemSalesDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomerActivityDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class PeakHourDto
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
    }
}
