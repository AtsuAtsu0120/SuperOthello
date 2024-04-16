using VContainer;
using VContainer.Unity;

namespace SuperOthello.Model
{
    public class OthelloLogic : IInitializable
    {
        private OthelloGame _game;
        
        [Inject] 
        public OthelloLogic(IObjectResolver _resolver)
        {
            _game = _resolver.Resolve<OthelloGame>();
        }
        public void Initialize()
        {
            
        }
    }
}