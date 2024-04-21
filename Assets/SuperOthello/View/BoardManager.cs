using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using SuperOthello.Model;
using UnityEngine;
using VContainer;

namespace SuperOthello.View
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private List<Cell> _cells;
        [SerializeField] private GameObject _piecePrefab;
        [SerializeField] private OutGameUI _outGameUI;
        
        private ISubscriber<CellState[,]> _boardSubscriber;
        private ISubscriber<(IEnumerable<(int row, int column)> canPutList, bool isBlackTurn)> _canPutSubscriber;
        private ISubscriber<bool> _gameEndSubscriber;
        
        [Inject]
        private void Constructor(ISubscriber<CellState[,]> boardSubscriber, ISubscriber<(IEnumerable<(int row, int column)> canPutList, bool isBlackTurn)> canPutSubscriber, ISubscriber<bool> gameEndSubscriber)
        {
            _boardSubscriber = boardSubscriber;
            _canPutSubscriber = canPutSubscriber;
            _gameEndSubscriber = gameEndSubscriber;
            
            Injected();
        }

        private void Injected()
        {
            _boardSubscriber.Subscribe(board => UpdateBoard(board));
            _canPutSubscriber.Subscribe((value) => ShowCanPutPosition(value.canPutList, value.isBlackTurn));
            _gameEndSubscriber.Subscribe(OnGameEnd);
        }

        private void UpdateBoard(in CellState[,] board)
        {
            if (board is null)
            {
                return;
            }

            for (var row = 0; row < OthelloGame.RowLength; row++)
            {
                for (var column = 0; column < OthelloGame.ColumnLength; column++)
                {
                    var index = column * 8 + row;
                    _cells[index].Put(board[row, column]);
                }
            }
        }

        private void ShowCanPutPosition(in IEnumerable<(int row, int column)> positionList, in bool isBlackTurn)
        {
            foreach (var cell in _cells)
            {
                var isCanPut = positionList.Any(position =>
                    position.row == cell.CellPosition.Row && position.column == cell.CellPosition.Column);
                cell.CanPutInfo = (isCanPut, isBlackTurn);
            }
        }

        private void OnGameEnd(bool isBlackWin)
        {
            _outGameUI.SetEndText(isBlackWin);
            _outGameUI.SetUIActive(true);
        }
    }
}