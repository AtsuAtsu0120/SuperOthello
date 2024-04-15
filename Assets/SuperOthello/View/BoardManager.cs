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
        private ISubscriber<IEnumerable<(int row, int column)>> _canPutSubscriber;
        
        [Inject]
        private void Constructor(ISubscriber<CellState[,]> boardSubscriber, ISubscriber<IEnumerable<(int row, int column)>> canPutSubscriber)
        {
            _boardSubscriber = boardSubscriber;
            _canPutSubscriber = canPutSubscriber;
            
            Injected();
        }

        private void Injected()
        {
            _boardSubscriber.Subscribe(board => UpdateBoard(board));
            _canPutSubscriber.Subscribe(list => ShowCanPutPosition(list));
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

        private void ShowCanPutPosition(in IEnumerable<(int row, int column)> positionList)
        {
            foreach (var (row, column) in positionList)
            {
                var index = column * 8 + row;
                _cells[index].ShowCanPutEffect();
            }
        }
    }
}