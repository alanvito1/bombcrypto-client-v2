namespace Game.Shared.DTOs
{
    public class LoginRequestDTO
    {
        public string WalletAddress { get; set; }
        public string Signature { get; set; }
        public string Message { get; set; }
    }
}
