using Crashmania.Models;
using PureMVC.Patterns.Proxy;

namespace Crashmania.PureMvc.Proxies
{
    public sealed class AuthProxy : Proxy
    {
        public const string Name = "AuthProxy";

        public AuthProxy() : base(Name)
        {
        }

        public PlayerProfile Profile { get; private set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public bool IsFirstLogin { get; private set; } = true;

        public void SetAuthenticated(AuthResponse response)
        {
            Profile = response.Profile;
            AccessToken = response.AccessToken;
            RefreshToken = response.RefreshToken;
            IsAuthenticated = response.Success;
            IsFirstLogin = response.Success && IsFirstLogin;
        }

        public void MarkFirstLoginSeen()
        {
            IsFirstLogin = false;
        }

        public void Clear()
        {
            Profile = null;
            AccessToken = null;
            RefreshToken = null;
            IsAuthenticated = false;
            IsFirstLogin = true;
        }
    }
}
