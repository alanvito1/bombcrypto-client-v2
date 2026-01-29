namespace Game.Database.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public required string WalletAddress { get; set; }
        public string? Nonce { get; set; }
        public decimal Gold { get; set; } = 1000;
        public decimal Bcoin { get; set; } = 50;
    }
}
