using System.Security.Claims;
using System.Threading.Tasks;
using Game.Server.Services;
using Game.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game.Server.Controllers
{
    [ApiController]
    [Route("user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetInfo()
        {
            var walletAddress = User.FindFirst("wallet")?.Value;
            if (string.IsNullOrEmpty(walletAddress)) return Unauthorized();

            var player = await _userService.GetByWalletAsync(walletAddress);
            if (player == null) return NotFound();

            return Ok(new UserInfoDTO
            {
                Id = player.Id.ToString(),
                Name = $"User-{player.Id}",
                WalletAddress = player.WalletAddress
            });
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var walletAddress = User.FindFirst("wallet")?.Value;
            if (string.IsNullOrEmpty(walletAddress)) return Unauthorized();

            var player = await _userService.GetByWalletAsync(walletAddress);
            if (player == null) return NotFound();

            return Ok(new UserBalanceDTO
            {
                Gold = player.Gold,
                Bcoin = player.Bcoin
            });
        }
    }
}
