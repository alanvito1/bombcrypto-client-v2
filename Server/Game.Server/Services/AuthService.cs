using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Game.Database;
using Game.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Nethereum.Signer;

namespace Game.Server.Services
{
    public interface IAuthService
    {
        Task<string> GenerateNonceAsync(string walletAddress);
        Task<string?> VerifyAndLoginAsync(string walletAddress, string signature, string message);
    }

    public class AuthService : IAuthService
    {
        private readonly GameDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly EthereumMessageSigner _signer;

        public AuthService(GameDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _signer = new EthereumMessageSigner();
        }

        public async Task<string> GenerateNonceAsync(string walletAddress)
        {
            var nonce = Guid.NewGuid().ToString();

            var player = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.WalletAddress == walletAddress);

            if (player == null)
            {
                player = new Player
                {
                    WalletAddress = walletAddress,
                    Nonce = nonce
                };
                _dbContext.Players.Add(player);
            }
            else
            {
                player.Nonce = nonce;
            }

            await _dbContext.SaveChangesAsync();
            return nonce;
        }

        public async Task<string?> VerifyAndLoginAsync(string walletAddress, string signature, string message)
        {
            try
            {
                var recoveredAddress = _signer.EncodeUTF8AndEcRecover(message, signature);

                if (!recoveredAddress.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.WalletAddress == walletAddress);

                if (player == null || player.Nonce != message)
                {
                    return null;
                }

                player.Nonce = Guid.NewGuid().ToString();
                await _dbContext.SaveChangesAsync();

                return GenerateJwtToken(player);
            }
            catch
            {
                return null;
            }
        }

        private string GenerateJwtToken(Player player)
        {
            var secret = _configuration["JwtSettings:Secret"] ?? "super_secret_key_for_local_development_only_12345";
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                    new Claim("wallet", player.WalletAddress)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
