using Azure.Core;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.DTOs.Auth;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services;
using RestaurantAPI.Services.Interfaces;
using RestaurantAPI.Utilities;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _context;
    private readonly RsaKeyService _rsaKeyService;

    public AuthService(IAuthRepository repository, IJwtService jwtService, ApplicationDbContext context,RsaKeyService rsaKeyService)
    {
        _repository = repository;
        _jwtService = jwtService;
        _context = context;
        _rsaKeyService = rsaKeyService;
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        // Check existing user
        var existingUser = await _repository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new Exception("Email already exists.");

        var roleEntity = await _context.Role.FirstOrDefaultAsync(r => r.RoleName.ToLower() == dto.Role.ToLower());

        if (roleEntity == null)
        {
            throw new Exception("Invalid role specified.");
        }

        var user = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = dto.Password, // will be hashed in repository
            RoleId = roleEntity.Id
        };

        var createdUser = await _repository.RegisterAsync(user);

        // Generate JWT
        return _jwtService.GenerateToken(createdUser);
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _repository.GetByEmailAsync(dto.Email);
        // Decrypt password using private key
        string decryptedRawPassword = _rsaKeyService.Decrypt(dto.Password);


        if (user == null || !PasswordHasher.VerifyPassword(decryptedRawPassword, user.Password))
            throw new Exception("Invalid credentials.");

        return _jwtService.GenerateToken(user);
    }
}
