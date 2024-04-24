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

#if UNITY_EDITOR
                        Utility.DrawRay(transform.position, (Vector3)nextDir, 1);
#endif

                        if (gridWorld.IsDiagonalDirectionObstructedAt(transform.position, nextDir))
                        {
                            nextDir *= 0f;
                        }
                        else if (
                              gridWorld.IsDirectionObstructedAt(transform.position, nextDir)
                              || gridWorld.IsPosOutsideAt(transform.position)
                          )
                        {
                            var gridPos = gridWorld.ConvertWorldPosToGridPos(transform.position);
                            var worldPos = gridWorld.ConvertGridPosToWorldPos(gridPos);
                            onDragCollided?.Invoke(worldPos + nextDir.normalized);

                            nextDir *= 0f;
                        }

                        transform.position += 100 * Time.deltaTime * (Vector3)nextDir;
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