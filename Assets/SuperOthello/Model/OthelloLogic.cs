using System.Collections.Generic;
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
        private OthelloGame _game;
        
        
        public void Initialize()
        {
            
        }
        
        [Inject] 
        public OthelloLogic(ISubscriber<CellPosition> putSubscriber, IPublisher<CellState[,]> boardPublisher, IPublisher<IEnumerable<(int row, int column)>> canPutPublisher)
        {
            _game = new(boardPublisher, canPutPublisher);
            _putSubscriber.Subscribe(_ => Debug.Log("aaa"));
        }
    }
}