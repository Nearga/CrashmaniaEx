using Crashmania.Models;
using Cysharp.Threading.Tasks;

namespace Crashmania.Services
{
    public interface IGameLoader
    {
        UniTask LoadGame(GameModel game);
        UniTask UnloadGame();
    }
}
