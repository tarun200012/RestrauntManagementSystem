namespace RestaurantAPI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Customer user);
    }

}
