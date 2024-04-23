using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class DragAndDrop : MonoBehaviour
{
    [Header("Injected Dependencies")]
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

    void Start()
    {
        _collider = GetComponent<Collider2D>();
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
                        PuzzleManager.Instance.CurrentBeingDragged = this;

                        deltaX = touchPos.x - transform.position.x;
                        deltaY = touchPos.y - transform.position.y;
                    }
                    break;

                case TouchPhase.Moved:
                    if (this == PuzzleManager.Instance.CurrentBeingDragged)
                    {
                        onDragMove?.Invoke();
                        var nextPos = new Vector2(touchPos.x - deltaX, touchPos.y - deltaY);
                        var nextDir = nextPos - (Vector2)transform.position;

                        if (
                             gridWorld.IsWorldDirObstructedAt(transform.position, nextDir)
                             || gridWorld.IsWorldPosOutsideAt(nextPos)
                         )
                        {
                            var val = gridWorld.GetWorldPosValueAt(
                                nextPos
                            );
                            if (val > 0)
                            {
                                onDragCollided?.Invoke(nextPos);
                            }
                            nextPos = transform.position;
                        }

                        transform.position = nextPos;
                    }
                    break;

                case TouchPhase.Ended:
                    if (this == PuzzleManager.Instance.CurrentBeingDragged)
                    {
                        onDragEnd?.Invoke();
                        IsOnDrag = false;
                        PuzzleManager.Instance.CurrentBeingDragged = null;

                        CalculateTargetPosition();
                        onDroppedToFloor?.Invoke(targetPosition);
                        LeanTween.move(gameObject, targetPosition, .1f).setOnComplete(() =>
                        {
                            // something in here
                        });
                    }
                    break;
            }
        }
    }

    void CalculateTargetPosition()
    {
        Vector2 gridPos = gridWorld.ConvertWorldPosToGridPos(transform.position);
        Vector2 flooredWorldPos = gridWorld.FindFlooredWorldPosAt(gridPos);
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