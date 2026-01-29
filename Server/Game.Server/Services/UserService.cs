using System.Threading.Tasks;
using Game.Database;
using Game.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Server.Services
{
    public interface IUserService
    {
        Task<Player?> GetByWalletAsync(string walletAddress);
    }

    public class UserService : IUserService
    {
        private readonly GameDbContext _dbContext;

        public UserService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Player?> GetByWalletAsync(string walletAddress)
        {
            return await _dbContext.Players.FirstOrDefaultAsync(p => p.WalletAddress == walletAddress);
        }
    }
}
