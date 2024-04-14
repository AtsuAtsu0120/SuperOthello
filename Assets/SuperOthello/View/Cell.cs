using SuperOthello.Model;
using UnityEngine;

namespace SuperOthello.View
{
    public class Cell : MonoBehaviour
    {
        [field:SerializeField] public int Row { get; private set; }
        [field:SerializeField] public int Column { get; private set; }

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
