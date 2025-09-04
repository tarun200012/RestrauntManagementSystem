namespace RestaurantAPI.Models.Common
{
    public class SortDescriptor
    {
        public string Field { get; set; } = string.Empty;
        public string Direction { get; set; } = "asc"; // asc or desc
    }

    public class FilterDescriptor
    {
        public string Field { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class PaginatedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public List<SortDescriptor> Sort { get; set; } = new();
        public List<FilterDescriptor> Filters { get; set; } = new();
    }

}
