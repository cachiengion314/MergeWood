using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DragAndDrop : MonoBehaviour
{
    [Header("Injected Dependencies")]
    public Vector3 targetPosition;
    public float snapToTargetValue;

    [Header("Components")]
    private Collider2D _collider;
    public bool IsOnDrag { get; private set; }

    [Header("Settings")]
    private float deltaX, deltaY;
    public bool locked;
    private Vector3 dir;
    [Range(0, 100)]
    [SerializeField] private float moveSpeed;
    private Vector3 nextMovePos;
    private bool shouldMove = true;

    void Start()
    {
        _collider = GetComponent<Collider2D>();
        targetPosition = new Vector3(0, -3, 0);
    }

    void Update()
    {
        DragDropControl();
        MoveToTarget();
    }

    void MoveToTarget()
    {
        if (IsOnDrag) return;
        if (!shouldMove) return;
        Vector3 distanceDir = targetPosition - transform.position;
        if (distanceDir.sqrMagnitude < snapToTargetValue)
        {
            shouldMove = false;
            LeanTween.move(gameObject, targetPosition, .05f);
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
                    shouldMove = true;
                    
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
}