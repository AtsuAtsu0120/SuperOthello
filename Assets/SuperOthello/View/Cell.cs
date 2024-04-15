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
        public CellState State { get; private set; }
        private readonly ReactiveProperty<bool> _canPut = new();

        private void Start()
        {
            _canPut.Subscribe(value => transform.GetChild(0).gameObject.SetActive(value));
        }

        public void Put(GameObject piecePrefab, CellState state)
        {
            State = state;
            if (state is CellState.Empty)
            {
                return;
            }

            Destroy(transform.GetChild(0).gameObject);
            var piece = Instantiate(piecePrefab, transform);

            if (state is CellState.Black)
            {
                piece.transform.Rotate(180, 0, 0);
            }
        }
    }
}
