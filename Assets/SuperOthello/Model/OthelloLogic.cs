using MessagePipe;
using R3;
using VContainer;
using VContainer.Unity;

namespace SuperOthello.Model
{
    public class OthelloLogic : IInitializable
    {
        private OthelloGame _game;
        private IObjectResolver _resolver;
        private ISubscriber<Unit> _restartGame;
        
        [Inject] 
        public OthelloLogic(IObjectResolver resolver, ISubscriber<Unit> restartGame)
        {
            _resolver = resolver;
            _restartGame = restartGame;
            
            InitGame();
            _restartGame.Subscribe(_ => InitGame());
        }

        private void InitGame()
        {
            _game?.Dispose();
            _game = _resolver.Resolve<OthelloGame>();
        }
        public void Initialize()
        {
            
        }
    }
}