namespace Crashmania.Models
{
    public sealed class AuthResponse
    {
        public bool Success;
        public string AccessToken;
        public string RefreshToken;
        public PlayerProfile Profile;
        public string ErrorMessage;
    }

    public sealed class PlayerProfile
    {
        public string Id;
        public string Email;
        public string DisplayName;
        public double BalanceCC;
        public double BalanceSC;
        public int VipTier;
    }
}
