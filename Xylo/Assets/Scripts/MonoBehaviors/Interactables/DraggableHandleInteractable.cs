using System;
using System.Collections;
using UnityEngine;

public class DraggableHandleInteractable : MonoBehaviour, IClickBehavior, IReleaseBehavior, IClickAwayBehavior {
    [SerializeField] private GameObject _graphicsHolderPrefab;
    private HandleGraphicsHolder _graphicsHolder;
    private DraggableInteractable _parentBlock;
    private Vector2 MousePosition { get { return ControlsManager.self.mousePosition; } }
    private Vector2 originalMousePosition;
    public Vector3 Direction { get; private set; }
    private bool _isDragging;
    public void Initialize(DraggableInteractable parentBlock, Vector3 direction) {
        _parentBlock = parentBlock;
        Direction = VectorUtils.GetRoundedVector(direction);
        transform.localPosition = -direction;

        _isDragging = false;

        _graphicsHolder = Instantiate(_graphicsHolderPrefab, transform).GetComponent<HandleGraphicsHolder>();
        _graphicsHolder.Initialize();
    }
    
    public void SetHandleGraphics(bool isActive, bool isGreyed = false) {
        _graphicsHolder.SetActive(isActive);
        _graphicsHolder.SetGrey(isGreyed);
    }

    public void DoClick() {
        _parentBlock.TurnHandlesOffExceptInDirection(Direction);
        originalMousePosition = MousePosition;
        StartCoroutine(Drag());
    }
    public void DoClickAway() {
        _parentBlock.DoClickAway();
    }
    public void DoRelease() {
        _parentBlock.originalPosition = VectorUtils.GetSnapToGridVector(_parentBlock.originalPosition, _parentBlock.transform.position);
        _parentBlock.DoClick();
        _isDragging = false;
    }

    private IEnumerator Drag() {
        _isDragging = true;

        while (_isDragging) {
            float z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 originalMousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(originalMousePosition.x, originalMousePosition.y, z));
            Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(MousePosition.x, MousePosition.y, z));

            float mouseDelta;
            Vector3 newBlockPosition = _parentBlock.originalPosition;

            if (VectorUtils.GetAbsVector(Direction).y == 1) {
                mouseDelta = mousePositionInWorld.y - originalMousePositionInWorld.y;
                newBlockPosition.y += mouseDelta;
            }
            else {
                mouseDelta = (mousePositionInWorld.x + mousePositionInWorld.z) - (originalMousePositionInWorld.x + originalMousePositionInWorld.z);
                if (VectorUtils.GetAbsVector(Direction).x == 1) {
                    newBlockPosition.x += mouseDelta;
                }
                if (VectorUtils.GetAbsVector(Direction).z == 1) {
                    newBlockPosition.z += mouseDelta;
                }
            }

            newBlockPosition = VectorUtils.GetSnapToGridVector(_parentBlock.originalPosition, newBlockPosition);

            if (IsNotJumpingBlocks(newBlockPosition) && !_parentBlock.IsCollidingAtPosition(newBlockPosition)) {
                _parentBlock.GetComponent<Rigidbody>().MovePosition(newBlockPosition);
                _parentBlock.TurnHandlesOffExceptInDirection(Direction);
            }

            yield return null;
        }
    }

    private bool IsNotJumpingBlocks(Vector3 targetPosition) {
        return
            Math.Abs(_parentBlock.transform.position.x - targetPosition.x) <= 1 &&
            Math.Abs(_parentBlock.transform.position.y - targetPosition.y) <= .5 &&
            Math.Abs(_parentBlock.transform.position.z - targetPosition.z) <= 1;
    }
}