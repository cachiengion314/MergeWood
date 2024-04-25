using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class DragAndDrop : MonoBehaviour
{
    [Header("Injected Dependencies")]
    [Tooltip("gridWorld will be injected via Instantiate method, not now.")]
    public GridWorld gridWorld;
    public Vector3 targetPosition;

    [Header("Components")]
    private Collider2D _collider;
    public bool IsOnDrag { get; private set; }

    [Header("Events")]
    public Action<Vector2> onDroppedToFloor;
    public Action onDragBegan;
    public Action onDragMove;
    public Action<Vector2> onDragCollided;
    public Action onDragEnd;

    [Header("Settings")]
    private float deltaX, deltaY;

    [Range(0, 100)]
    [SerializeField] private float moveSpeed;
    Rigidbody2D _rig;

    void Start()
    {
        _collider = GetComponent<Collider2D>();
        _rig = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        DragDropControl();
#if UNITY_EDITOR
        DrawGrabedBlock();
#endif
    }

    void DragDropControl()
    {
        if (LevelManager.Instance.GetGameState() != GameState.Gameplay) return;
        if (PuzzleManager.Instance.IsTweening) return;

        if (Input.touchCount <= 0)
        {
            _rig.velocity = Vector3.zero;
            _rig.isKinematic = true;
            onDragEnd?.Invoke();
            if (this == PuzzleManager.Instance.CurrentBeingDragged)
            {
                IsOnDrag = false;
                PuzzleManager.Instance.CurrentBeingDragged = null;

                CalculateTargetPosition();
                onDroppedToFloor?.Invoke(targetPosition);
            }
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (_collider == Physics2D.OverlapPoint(touchPos))
                    {
                        _rig.isKinematic = false;
                        onDragBegan?.Invoke();
                        IsOnDrag = true;
                        PuzzleManager.Instance.CurrentBeingDragged = this;

                        deltaX = touchPos.x - transform.position.x;
                        deltaY = touchPos.y - transform.position.y;
                    }
                    break;

                case TouchPhase.Moved:
                    if (this == PuzzleManager.Instance.CurrentBeingDragged)
                    {
                        onDragMove?.Invoke();
                        var _touchPos = new Vector2(touchPos.x - deltaX, touchPos.y - deltaY);
                        var nextDir = _touchPos - (Vector2)transform.position;

#if UNITY_EDITOR
                        Utility.DrawRay(transform.position, (Vector3)nextDir, 1);
#endif

                        // r.AddForce(nextDir.normalized * 45, ForceMode2D.Force);
                        var nextPos = transform.position + 80 * Time.deltaTime * (Vector3)nextDir;

                        _rig.MovePosition(nextPos);
                    }
                    break;
            }
        }
    }

    void CalculateTargetPosition()
    {
        Vector2 gridPos = gridWorld.ConvertWorldPosToGridPos(transform.position);
        Vector2 flooredWorldPos = gridWorld.FindFlooredPosAt(gridPos);
        targetPosition = new Vector3(flooredWorldPos.x, flooredWorldPos.y);
    }

    /// <summary>
    /// only for debug
    /// </summary>
    void DrawGrabedBlock()
    {
        if (!IsOnDrag) return;
        Vector2 gridPos = gridWorld.ConvertWorldPosToGridPos(transform.position);
        Vector2 worldPos = gridWorld.ConvertGridPosToWorldPos(gridPos);
        Utility.DrawQuad(worldPos, .5f, 2);
    }
}