using Game.Server.Services;
using Game.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Game.Server.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("nonce/{walletAddress}")]
        public async Task<IActionResult> GetNonce(string walletAddress)
        {
            var nonce = await _authService.GenerateNonceAsync(walletAddress);
            return Ok(new { nonce });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var token = await _authService.VerifyAndLoginAsync(request.WalletAddress, request.Signature, request.Message);

            if (token == null)
            {
                return Unauthorized("Invalid signature or nonce");
            }

            return Ok(new { token });
        }
    }
}
