using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class DragAndDrop : MonoBehaviour
{
    [Header("Injected Dependencies")]
    public Vector3 targetPosition;

    [Header("Components")]
    private Collider2D _collider;
    public bool IsOnDrag { get; private set; }

    [Header("Events")]
    public Action<Vector2> onDroppedToFloor;
    public Action onDragBegan;
    public Action onDragMove;
    public Action onDragEnd;

    [Header("Settings")]
    private float deltaX, deltaY;

    [Range(0, 100)]
    [SerializeField] private float moveSpeed;

    void Start()
    {
        _collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        DragDropControl();

        DrawGrabedBlock();
    }

    void DragDropControl()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (_collider == Physics2D.OverlapPoint(touchPos))
                    {
                        onDragBegan?.Invoke();
                        IsOnDrag = true;
                        SpawnPuzzleBlocks.Instance.CurrentBeingDragged = this;

                        deltaX = touchPos.x - transform.position.x;
                        deltaY = touchPos.y - transform.position.y;
                    }
                    break;

                case TouchPhase.Moved:
                    if (this == SpawnPuzzleBlocks.Instance.CurrentBeingDragged)
                    {
                        onDragMove?.Invoke();
                        var nextPos = new Vector2(touchPos.x - deltaX, touchPos.y - deltaY);
                        var nextDir = nextPos - (Vector2)transform.position;
                        if (
                            GridWorld.Instance.IsWorldDirObstructedAt(transform.position, nextDir)
                            || GridWorld.Instance.IsWorldPosOutsideAt(nextPos)
                        )
                        {
                            nextPos = transform.position;
                        }
                        transform.position = nextPos;
                    }
                    break;

                case TouchPhase.Ended:
                    if (this == SpawnPuzzleBlocks.Instance.CurrentBeingDragged)
                    {
                        onDragEnd?.Invoke();
                        IsOnDrag = false;
                        SpawnPuzzleBlocks.Instance.CurrentBeingDragged = null;

                        CalculateTargetPosition();
                        LeanTween.move(gameObject, targetPosition, .1f).setOnComplete(() =>
                        {
                            onDroppedToFloor?.Invoke(targetPosition);
                        });
                    }
                    break;
            }
        }
    }

    void CalculateTargetPosition()
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(transform.position, GridWorld.Instance.Offset);
        Vector2 flooredWorldPos = GridWorld.Instance.FindFlooredWorldPosAt(gridPos);
        targetPosition = new Vector3(flooredWorldPos.x, flooredWorldPos.y);
    }

    /// <summary>
    /// only for debug
    /// </summary>
    void DrawGrabedBlock()
    {
        if (!IsOnDrag) return;
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(transform.position, GridWorld.Instance.Offset);
        Vector2 worldPos = GridUtility.ConvertGridPosToWorldPos(gridPos, GridWorld.Instance.Offset);
        Utility.DrawQuad(worldPos, .5f, 2);
    }
}