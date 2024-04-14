using System.Collections.Generic;
using MessagePipe;
using SuperOthello.Model;
using UnityEngine;
using VContainer;

namespace SuperOthello.View
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private List<Cell> _cells;
        [SerializeField] private GameObject _piece;
        
        private ISubscriber<CellState[,]> _boardSubscriber;
        
        [Inject]
        private void Constructor(ISubscriber<CellState[,]> boardSubscriber)
        {
            _boardSubscriber = boardSubscriber;
            
            Injected();
        }

        private void Injected()
        {
            _boardSubscriber.Subscribe(board => UpdateBoard(board));
        }

        private void UpdateBoard(in CellState[,] board)
        {
            if (board is null)
            {
                return;
            }
            
            int index = 0;
            foreach (var state in board)
            {
                _cells[index].Put(_piece, state);

                index++;
            }
        }
    }
}