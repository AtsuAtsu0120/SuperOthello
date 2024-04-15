using MessagePipe;
using SuperOthello.Model;
using SuperOthello.View;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace SuperOthello.Provider
{
    public class BoardInputProvider : MonoBehaviour
    {
        private IPublisher<CellPosition> _putPublisher;
        
        private OthelloInputActions _actions;
        private RaycastHit[] _hits;

        [Inject]
        private void Constructor(IPublisher<CellPosition> putPublisher)
        {
            _putPublisher = putPublisher;
        }
        
        private void Awake()
        {
            InitInputAction();;
            
            _hits = new RaycastHit[1];
        }

        private void InitInputAction()
        {
            _actions = new OthelloInputActions();
            _actions.Player.Enable();
            _actions.Player.Click.performed += PutPiece;
        }

        private void PutPiece(InputAction.CallbackContext _)
        {
            var mousePosition = _actions.Player.Position.ReadValue<Vector2>();
            if (Physics.RaycastNonAlloc(Camera.main.ScreenPointToRay(mousePosition), _hits) > 0)
            {
                if (_hits[0].collider.TryGetComponent<Cell>(out var component) && component.CanPut)
                {
                    _putPublisher.Publish(component.CellPosition);
                }
            }
        }
    }
}
