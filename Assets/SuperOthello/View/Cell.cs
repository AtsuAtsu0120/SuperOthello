using System;
using R3;
using SuperOthello.Model;
using UnityEngine;

namespace SuperOthello.View
{
    public class Cell : MonoBehaviour
    {
        public bool CanPut
        {
            get => _canPut.Value;
            set => _canPut.Value = value;
        } 
        [field: SerializeField] public CellPosition CellPosition { get; private set; }
        private ReactiveProperty<bool> _canPut = new();

        private void Start()
        {
            _canPut.Subscribe(value => transform.GetChild(0).gameObject.SetActive(value));
        }

        public void Put(GameObject piecePrefab, CellState state)
        {
            if (state is CellState.Empty)
            {
                return;
            }
            var piece = Instantiate(piecePrefab, transform);

            if (state == CellState.Black)
            {
                piece.transform.Rotate(180, 0, 0);
            }
        }
    }
}
