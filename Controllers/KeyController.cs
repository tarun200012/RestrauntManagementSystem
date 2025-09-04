using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Services;

[ApiController]
[Route("api/key")]
public class KeyController : ControllerBase
{
    private readonly RsaKeyService _rsaKeyService;

    public KeyController(RsaKeyService rsaKeyService)
    {
        _rsaKeyService = rsaKeyService;
    }

    [HttpGet("public")]
    public IActionResult GetPublicKey()
    {
        return Ok(new { PublicKey = _rsaKeyService.GetPublicKey() });
    }
}
