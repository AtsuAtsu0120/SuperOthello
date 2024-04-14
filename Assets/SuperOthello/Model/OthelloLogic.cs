using MessagePipe;
using UnityEngine;
using VContainer;
using R3;
using VContainer.Unity;

namespace SuperOthello.Model
{
    public class OthelloLogic : IInitializable
    {
        private readonly ISubscriber<CellPosition> _putSubscriber;
        private readonly IPublisher<CellState[,]> _boardPublisher;

        private OthelloGame _game;
        
        
        public void Initialize()
        {
            
        }
        
        [Inject] 
        public OthelloLogic(ISubscriber<CellPosition> putSubscriber, IPublisher<CellState[,]> boardPublisher)
        {
            _putSubscriber = putSubscriber;
            _boardPublisher = boardPublisher;
            
            Injected();
        }

        private void Injected()
        {
            _putSubscriber.Subscribe(_ => Debug.Log("aaa"));

            _game = new(_boardPublisher);
        }
    }
}