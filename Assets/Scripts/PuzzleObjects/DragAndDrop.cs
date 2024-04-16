using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class DragAndDrop : MonoBehaviour
{
    [Header("Injected Dependencies")]
    public Vector3 targetPosition;
    public float snapToTargetValue;

    [Header("Components")]
    private Collider2D _collider;
    public bool IsOnDrag { get; private set; }

    [Header("Events")]
    public Action<Vector2> onMovedToTarget;

    [Header("Settings")]
    private float deltaX, deltaY;
    public bool locked;
    private Vector3 dir;
    [Range(0, 100)]
    [SerializeField] private float moveSpeed;
    private Vector3 nextMovePos;
    private bool isMoveToTarget = true;

    void Start()
    {
        _collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        DragDropControl();
        MoveToTarget();
        CalculateTargetPosition();

        DrawGrabedBlock();
    }

    void MoveToTarget()
    {
        if (IsOnDrag) return;
        if (isMoveToTarget) return;
        Vector3 distanceDir = targetPosition - transform.position;
        if (distanceDir.sqrMagnitude < snapToTargetValue)
        {
            isMoveToTarget = true;
            LeanTween.move(gameObject, targetPosition, .02f);

            onMovedToTarget?.Invoke(targetPosition);
            GridWorld.Instance.SetWorldPosBlockedAt(targetPosition, 1);
            return;
        }

        dir = new Vector3(distanceDir.normalized.x, distanceDir.normalized.y, 0);
        nextMovePos = transform.position + (moveSpeed * Time.deltaTime * dir);
        transform.position = nextMovePos;
    }

    void DragDropControl()
    {
        if (Input.touchCount > 0 && !locked)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    IsOnDrag = true;
                    isMoveToTarget = false;

                    if (_collider == Physics2D.OverlapPoint(touchPos))
                    {
                        deltaX = touchPos.x - transform.position.x;
                        deltaY = touchPos.y - transform.position.y;
                    }
                    break;

                case TouchPhase.Moved:
                    if (_collider == Physics2D.OverlapPoint(touchPos))
                    {
                        transform.position = new Vector2(touchPos.x - deltaX, touchPos.y - deltaY);
                    }
                    break;

                case TouchPhase.Ended:
                    IsOnDrag = false;
                    if (Mathf.Abs(transform.position.x - targetPosition.x) <= 1f &&
                        Mathf.Abs(transform.position.y - targetPosition.y) <= 1f)
                    {

                    }
                    break;
            }
        }
    }

    void CalculateTargetPosition()
    {
        if (!IsOnDrag) return;
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(transform.position, GridWorld.Instance.offset);
        Vector2 flooredWorldPos = GridWorld.Instance.FindFlooredWorldPosAt(gridPos);
        targetPosition = new Vector3(flooredWorldPos.x, flooredWorldPos.y);
    }

    /// <summary>
    /// only for debug
    /// </summary>
    void DrawGrabedBlock()
    {
        if (IsOnDrag)
        {
            Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(transform.position, GridWorld.Instance.offset);
            Vector2 worldPos = GridUtility.ConvertGridPosToWorldPos(gridPos, GridWorld.Instance.offset);
            Utility.DrawQuad(worldPos, .5f, 2);
        }
    }
}