namespace RestaurantAPI.DTOs.Common
{
    public class PaginatedResponse<T>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new();
    }

}
