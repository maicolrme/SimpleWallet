using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Authentication;
using SimpleWallet.Extensions;
using SimpleWallet.Services;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers.Api;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class ApiTokenController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IApiTokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public ApiTokenController(
        IAuthService authService,
        IApiTokenService tokenService,
        IUserService userService,
        IConfiguration configuration)
    {
        _authService = authService;
        _tokenService = tokenService;
        _userService = userService;
        _configuration = configuration;
    }

    /// <summary>
    /// Exchanges email and password for a bearer token.
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> CreateToken(TokenRequest request)
    {
        var user = await _authService.ValidateAsync(request.Email, request.Password);

        if (user is null)
            return Unauthorized(new { error = "Invalid email or password." });

        var hours = _configuration.GetValue<int>("Api:TokenLifetimeHours", 24);
        var lifetime = TimeSpan.FromHours(hours <= 0 ? 24 : hours);

        return Ok(new TokenResponse
        {
            Token = _tokenService.IssueToken(user.Id, lifetime),
            ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime)
        });
    }

    /// <summary>
    /// Current user profile and wallet.
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var user = await _userService.GetByIdAsync(User.GetUserId());

        if (user is null)
            return NotFound(new { error = "User not found." });

        return Ok(ToDto(user));
    }

    internal static UserProfileDto ToDto(Models.User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        Wallet = user.Wallet is null ? null : new WalletDto
        {
            Id = user.Wallet.Id,
            Address = user.Wallet.Address,
            Balance = user.Wallet.Balance,
            Currency = user.Wallet.Currency,
            CreatedAt = user.Wallet.CreatedAt
        }
    };
}
